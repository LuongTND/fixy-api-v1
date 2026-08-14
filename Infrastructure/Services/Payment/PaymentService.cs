using System.Text.Json;
using Application.Common;
using Application.DTOs.Payment;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Booking;
using Application.Interfaces.Services.Payment;
using Domain.Entity;
using Domain.Enum;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Net.payOS.Types;

namespace Infrastructure.Services.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentOrderRepository _paymentOrderRepository;

        private readonly ICustomerProfileRepository _customerProfileRepository;

        private readonly IPaymentGatewayFactory _paymentGatewayFactory;

        private readonly IBookingRepository _bookingRepository;

        private readonly IWalletService _walletService;

        private readonly IUnitOfWork _unitOfWork;

        private readonly IBookingService _bookingService;

        private readonly INotificationService _notificationService;

        private readonly IHubContext<NotificationHub> _hubContext;

        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentOrderRepository paymentOrderRepository,
            ICustomerProfileRepository customerProfileRepository,
            IPaymentGatewayFactory paymentGatewayFactory,
            IBookingRepository bookingRepository,
            IWalletService walletService,
            IUnitOfWork unitOfWork,
            IBookingService bookingService,
            INotificationService notificationService,
            IHubContext<NotificationHub> hubContext,
            ILogger<PaymentService> logger
        )
        {
            _paymentOrderRepository = paymentOrderRepository;

            _customerProfileRepository = customerProfileRepository;

            _paymentGatewayFactory = paymentGatewayFactory;

            _bookingRepository = bookingRepository;

            _walletService = walletService;

            _unitOfWork = unitOfWork;

            _bookingService = bookingService;

            _notificationService = notificationService;

            _hubContext = hubContext;

            _logger = logger;
        }

        public async Task<OperationResult<string>> CreateTopUpPaymentUrlAsync(
            Guid userId,
            long amount,
            PaymentMethod method,
            CancellationToken cancellationToken
        )
        {
            if (amount <= 0)
            {
                return OperationResult<string>.Failure("Invalid amount");
            }

            var order = new PaymentOrder
            {
                UserId = userId,

                Amount = amount,
                DiscountAmount = 0,
                FinalAmount = amount,

                Method = method,
                Status = PaymentStatus.Pending,

                Type = PaymentOrderType.WalletTopUp,
            };

            await _paymentOrderRepository.AddAsync(order, cancellationToken);

            var gateway = _paymentGatewayFactory.Get(method);

            var paymentUrl = await gateway.CreatePaymentUrlAsync(order, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return OperationResult<string>.Success(paymentUrl);
        }

        public async Task<OperationResult<string>> CreateBookingPaymentUrlAsync(
            Guid bookingId,
            Guid userId,
            PaymentMethod method,
            CancellationToken cancellationToken
        )
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);

            if (booking == null)
            {
                return OperationResult<string>.Failure("Booking not found");
            }

            var customerProfile = await _customerProfileRepository.GetByUserIdAsync(
                userId,
                cancellationToken
            );

            if (customerProfile == null)
            {
                return OperationResult<string>.Failure("Customer profile not found");
            }

            if (booking.CustomerProfileId != customerProfile.Id)
            {
                return OperationResult<string>.Failure("Forbidden");
            }

            if (booking.FinalPrice == null || booking.FinalPrice <= 0)
            {
                return OperationResult<string>.Failure("Invalid booking price");
            }



            var existedOrder = await _paymentOrderRepository.GetBookingPaymentOrderAsync(
                bookingId,
                cancellationToken
            );

            if (existedOrder != null && existedOrder.Status == PaymentStatus.Paid)
            {
                return OperationResult<string>.Failure("Booking already paid");
            }

            PaymentOrder order;

            if (existedOrder != null)
            {
                order = existedOrder;

                order.Method = method;

                order.Status = PaymentStatus.Pending;

                _paymentOrderRepository.Update(order);
            }
            else
            {
                order = new PaymentOrder
                {
                    BookingId = booking.Id,

                    UserId = userId,

                    Amount = booking.FinalPrice.Value,

                    DiscountAmount = 0,

                    FinalAmount = booking.FinalPrice.Value,

                    Method = method,

                    Status = PaymentStatus.Pending,

                    Type = PaymentOrderType.BookingPayment,
                };

                await _paymentOrderRepository.AddAsync(order, cancellationToken);
            }

            // Cash payments don't go through an online gateway.
            // The order stays Pending until cash is actually collected after the service.
            if (method == PaymentMethod.Cash)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                if (order.BookingId.HasValue)
                {
                    await _bookingService.ConfirmPaymentAsync(
                        order.BookingId.Value,
                        cancellationToken
                    );
                }

                return OperationResult<string>.Success(string.Empty);
            }

            var gateway = _paymentGatewayFactory.Get(method);

            var paymentUrl = await gateway.CreatePaymentUrlAsync(order, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return OperationResult<string>.Success(paymentUrl);
        }

        public async Task<OperationResult<bool>> HandleCallbackAsync(
            PaymentMethod method,
            Dictionary<string, string> response,
            CancellationToken cancellationToken
        )
        {
            var gateway = _paymentGatewayFactory.Get(method);

            var valid = gateway.VerifySignature(response);

            if (!valid)
            {
                return OperationResult<bool>.Failure("Invalid signature");
            }

            PaymentOrder? order = null;

            string? transactionId = null;

            bool paymentSuccess = false;

            switch (method)
            {
                case PaymentMethod.Vnpay:
                {
                    var orderId = Guid.Parse(response["vnp_TxnRef"]);

                    order = await _paymentOrderRepository.GetByIdAsync(orderId, cancellationToken);

                    transactionId = response["vnp_TransactionNo"];

                    paymentSuccess = response["vnp_ResponseCode"] == "00";

                    break;
                }

                case PaymentMethod.Momo:
                {
                    var orderId = Guid.Parse(response["orderId"]);

                    order = await _paymentOrderRepository.GetByIdAsync(orderId, cancellationToken);

                    transactionId = response["transId"];

                    paymentSuccess = response["resultCode"] == "0";

                    break;
                }

                default:

                    return OperationResult<bool>.Failure("Unsupported payment method");
            }

            if (order == null)
            {
                return OperationResult<bool>.Failure("Payment order not found");
            }

            if (order.Status == PaymentStatus.Paid)
            {
                return OperationResult<bool>.Success(true, "Payment already processed");
            }

            if (!paymentSuccess)
            {
                order.Status = PaymentStatus.Failed;

                _paymentOrderRepository.Update(order);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return OperationResult<bool>.Failure("Payment failed");
            }

            order.Status = PaymentStatus.Paid;

            order.PaidAt = DateTime.UtcNow;

            order.ExternalTransactionId = transactionId;

            order.GatewayResponse = JsonSerializer.Serialize(response);

            _paymentOrderRepository.Update(order);

            await ProcessSuccessfulPaymentAsync(order, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return OperationResult<bool>.Success(true, "Payment success");
        }

        private async Task ProcessSuccessfulPaymentAsync(
            PaymentOrder order,
            CancellationToken cancellationToken
        )
        {
            switch (order.Type)
            {
                case PaymentOrderType.WalletTopUp:

                    await _walletService.TopUpAsync(
                        order.UserId,
                        order.FinalAmount,
                        $"{order.Method} Topup #{order.Id}",
                        cancellationToken
                    );

                    break;

                case PaymentOrderType.BookingPayment:

                    if (order.BookingId.HasValue)
                    {
                        await _bookingService.ConfirmPaymentAsync(
                            order.BookingId.Value,
                            cancellationToken
                        );
                    }

                    break;

                default:

                    throw new Exception($"Unsupported payment type: {order.Type}");
            }
        }

        public async Task<OperationResult<bool>> HandlePayOSCallbackAsync(
            PayOSCallbackDto callback,
            CancellationToken cancellationToken
        )
        {
            var data = callback.Data;

            if (data == null || data.OrderCode <= 0)
            {
                _logger.LogInformation("PayOS webhook received empty/invalid data payload. Acknowledging as test ping.");
                return OperationResult<bool>.Success(true, "Callback acknowledged");
            }

            // 1. Verify Webhook Signature using PayOS SDK (chống giả mạo request)
            try
            {
                var payosService = _paymentGatewayFactory.Get(PaymentMethod.PayOS) as PayOSService;
                if (payosService != null)
                {
                    var jsonString = JsonSerializer.Serialize(callback);
                    var webhookBody = JsonSerializer.Deserialize<WebhookType>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (webhookBody != null)
                    {
                        payosService.VerifyWebhookData(webhookBody);
                        _logger.LogInformation("PayOS webhook signature verified for OrderCode: {OrderCode}", data.OrderCode);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "PayOS webhook signature verification failed for OrderCode: {OrderCode}", data.OrderCode);

                // Check if this is a test webhook (order code not found in DB)
                var testCheckOrder = await _paymentOrderRepository.GetByGatewayOrderCodeAsync(
                    data.OrderCode,
                    cancellationToken
                );
                if (testCheckOrder == null)
                {
                    _logger.LogInformation("PayOS test webhook ping detected (order {OrderCode} not found). Returning 200 OK.", data.OrderCode);
                    return OperationResult<bool>.Success(true, "Test webhook acknowledged");
                }

                return OperationResult<bool>.Failure("Invalid PayOS webhook signature");
            }

            // 2. Lookup the payment order
            var order = await _paymentOrderRepository.GetByGatewayOrderCodeAsync(
                data.OrderCode,
                cancellationToken
            );

            if (order == null)
            {
                _logger.LogInformation("PayOS webhook: Payment order not found for OrderCode {OrderCode}. Returning 200 OK for test request.", data.OrderCode);
                return OperationResult<bool>.Success(true, "Order not found, test request acknowledged");
            }

            // 3. Idempotency check — already processed
            if (order.Status == PaymentStatus.Paid)
                return OperationResult<bool>.Success(true, "Already processed");

            var isSuccess = data.Code == "00";

            order.GatewayResponse = JsonSerializer.Serialize(callback);

            if (!isSuccess)
            {
                order.Status = PaymentStatus.Failed;
                _paymentOrderRepository.Update(order);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return OperationResult<bool>.Success(false, "Payment failed status recorded");
            }

            // 4. Mark as Paid
            order.Status = PaymentStatus.Paid;
            order.PaidAt = DateTime.UtcNow;
            order.ExternalTransactionId = data.OrderCode.ToString();

            // Store the bank reference code for audit trail
            if (!string.IsNullOrEmpty(data.Reference))
            {
                order.GatewayRef = data.Reference;
            }

            _paymentOrderRepository.Update(order);

            // 5. Process business logic (TopUp wallet or Confirm booking)
            //    IMPORTANT: Do this BEFORE SaveChangesAsync so both PaymentOrder update
            //    and Booking status change are committed atomically.
            await ProcessSuccessfulPaymentAsync(order, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 6. Send real-time notification to Mobile App via SignalR
            try
            {
                await _hubContext.Clients
                    .User(order.UserId.ToString())
                    .SendAsync("PaymentCompleted", new
                    {
                        OrderId = order.Id,
                        BookingId = order.BookingId,
                        Amount = order.FinalAmount,
                        Status = "Paid",
                        Reference = data.Reference
                    }, cancellationToken);

                _logger.LogInformation(
                    "SignalR PaymentCompleted sent to user {UserId} for OrderCode {OrderCode}",
                    order.UserId, data.OrderCode);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to send SignalR PaymentCompleted to user {UserId}",
                    order.UserId);
            }

            // 7. Send Push Notification (Firebase FCM) + save to notification DB
            try
            {
                var notifTitle = order.Type == PaymentOrderType.WalletTopUp
                    ? "Nạp ví thành công!"
                    : "Thanh toán thành công!";

                var notifBody = order.Type == PaymentOrderType.WalletTopUp
                    ? $"Bạn đã nạp thành công {order.FinalAmount:N0}đ vào Ví Fixy qua PayOS."
                    : $"Đơn dịch vụ của bạn đã được thanh toán {order.FinalAmount:N0}đ và đang chờ Kỹ thuật viên tiếp nhận.";

                var deepLink = order.BookingId.HasValue
                    ? $"/booking-detail?bookingId={order.BookingId.Value}"
                    : "/wallet";

                await _notificationService.SendNotificationAsync(
                    order.UserId,
                    NotificationType.Payment,
                    notifTitle,
                    notifBody,
                    deepLink,
                    new { orderId = order.Id, bookingId = order.BookingId, amount = order.FinalAmount },
                    null,
                    cancellationToken
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to send payment notification for OrderCode {OrderCode}",
                    data.OrderCode);
            }

            return OperationResult<bool>.Success(true, "Payment success");
        }
    }
}

