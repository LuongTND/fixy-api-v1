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

        public PaymentService(
            IPaymentOrderRepository paymentOrderRepository,
            ICustomerProfileRepository customerProfileRepository,
            IPaymentGatewayFactory paymentGatewayFactory,
            IBookingRepository bookingRepository,
            IWalletService walletService,
            IUnitOfWork unitOfWork,
            IBookingService bookingService
        )
        {
            _paymentOrderRepository = paymentOrderRepository;

            _customerProfileRepository = customerProfileRepository;

            _paymentGatewayFactory = paymentGatewayFactory;

            _bookingRepository = bookingRepository;

            _walletService = walletService;

            _unitOfWork = unitOfWork;

            _bookingService = bookingService;
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
                return OperationResult<bool>.Failure("Invalid callback");

            var order = await _paymentOrderRepository.GetByGatewayOrderCodeAsync(
                data.OrderCode,
                cancellationToken
            );

            if (order == null)
                return OperationResult<bool>.Failure("Payment order not found");

            if (order.Status == PaymentStatus.Paid)
                return OperationResult<bool>.Success(true, "Already processed");

            var isSuccess = data.Code == "00";

            order.GatewayResponse = JsonSerializer.Serialize(callback);

            if (!isSuccess)
            {
                order.Status = PaymentStatus.Failed;
                _paymentOrderRepository.Update(order);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return OperationResult<bool>.Failure("Payment failed");
            }

            order.Status = PaymentStatus.Paid;
            order.PaidAt = DateTime.UtcNow;
            order.ExternalTransactionId = data.OrderCode.ToString();

            _paymentOrderRepository.Update(order);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await ProcessSuccessfulPaymentAsync(order, CancellationToken.None);

            return OperationResult<bool>.Success(true, "Payment success");
        }
    }
}
