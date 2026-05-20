using System.Text.Json;
using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Payment;
using Application.Interfaces.Services.Booking;
using Domain.Entity;
using Domain.Enum;

namespace Infrastructure.Services.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentOrderRepository _paymentOrderRepository;

        private readonly IPaymentGatewayFactory _paymentGatewayFactory;
        private readonly IBookingRepository _bookingRepository;

        private readonly IWalletService _walletService;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IBookingService _bookingService;

        public PaymentService(
            IPaymentOrderRepository paymentOrderRepository,
            IPaymentGatewayFactory paymentGatewayFactory,
            IBookingRepository bookingRepository,
            IWalletService walletService,
            IUnitOfWork unitOfWork,
            IBookingService bookingService
        )
        {
            _paymentOrderRepository = paymentOrderRepository;
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

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var gateway = _paymentGatewayFactory.Get(method);

            var paymentUrl = await gateway.CreatePaymentUrlAsync(order, cancellationToken);

            return OperationResult<string>.Success(paymentUrl);
        }

        public async Task<OperationResult<string>> CreateBookingVnPayUrlAsync(
            Guid bookingId,
            Guid userId,
            CancellationToken cancellationToken
        )
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);

            if (booking == null)
            {
                return OperationResult<string>.Failure("Booking not found");
            }

            if (booking.CustomerId != userId)
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

                order.Method = PaymentMethod.Vnpay;
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

                    Method = PaymentMethod.Vnpay,
                    Status = PaymentStatus.Pending,

                    Type = PaymentOrderType.BookingPayment,
                };

                await _paymentOrderRepository.AddAsync(order, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var gateway = _paymentGatewayFactory.Get(PaymentMethod.Vnpay);

            var paymentUrl = await gateway.CreatePaymentUrlAsync(order, cancellationToken);

            return OperationResult<string>.Success(paymentUrl);
        }

        public async Task HandleMoMoReturnAsync(
            Dictionary<string, string> response,
            CancellationToken cancellationToken
        )
        {
            var gateway = _paymentGatewayFactory.Get(PaymentMethod.Momo);

            var isValid = gateway.VerifySignature(response);

            if (!isValid)
            {
                throw new Exception("Invalid MoMo signature");
            }

            var resultCode = response["resultCode"];

            if (resultCode != "0")
            {
                throw new Exception("Payment failed");
            }

            var orderId = Guid.Parse(response["orderId"]);

            var order = await _paymentOrderRepository.GetByIdAsync(orderId, cancellationToken);

            if (order == null)
            {
                throw new Exception("Payment order not found");
            }

            if (order.Status == PaymentStatus.Paid)
            {
                return;
            }

            order.Status = PaymentStatus.Paid;

            order.PaidAt = DateTime.UtcNow;

            order.ExternalTransactionId = response["transId"];

            order.GatewayResponse = JsonSerializer.Serialize(response);

            _paymentOrderRepository.Update(order);

            await ProcessSuccessfulPaymentAsync(order, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<OperationResult<bool>> HandleVnPayCallbackAsync(
            Dictionary<string, string> response,
            CancellationToken cancellationToken
        )
        {
            var gateway = _paymentGatewayFactory.Get(PaymentMethod.Vnpay);

            var valid = gateway.VerifySignature(response);

            if (!valid)
            {
                return OperationResult<bool>.Failure("Invalid signature");
            }

            var orderId = Guid.Parse(response["vnp_TxnRef"]);

            var order = await _paymentOrderRepository.GetByIdAsync(orderId, cancellationToken);

            if (order == null)
            {
                return OperationResult<bool>.Failure("Payment order not found");
            }

            if (order.Status == PaymentStatus.Paid)
            {
                return OperationResult<bool>.Success(true, "Payment already processed");
            }

            var responseCode = response["vnp_ResponseCode"];

            if (responseCode != "00")
            {
                order.Status = PaymentStatus.Failed;

                _paymentOrderRepository.Update(order);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return OperationResult<bool>.Failure("Payment failed");
            }

            order.Status = PaymentStatus.Paid;

            order.PaidAt = DateTime.UtcNow;

            order.ExternalTransactionId = response["vnp_TransactionNo"];

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
                        await _bookingService.ConfirmPaymentAsync(order.BookingId.Value, cancellationToken);
                    }

                    break;

                default:

                    throw new Exception($"Unsupported payment type: {order.Type}");
            }
        }
    }
}
