using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Payment;
using Domain.Entity;
using Domain.Enum;

namespace Infrastructure.Services.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentOrderRepository _paymentOrderRepository;

        private readonly IPaymentGatewayFactory _paymentGatewayFactory;

        private readonly IWalletService _walletService;

        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(
            IPaymentOrderRepository paymentOrderRepository,
            IPaymentGatewayFactory paymentGatewayFactory,
            IWalletService walletService,
            IUnitOfWork unitOfWork
        )
        {
            _paymentOrderRepository = paymentOrderRepository;
            _paymentGatewayFactory = paymentGatewayFactory;
            _walletService = walletService;
            _unitOfWork = unitOfWork;
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
                FinalAmount = amount,
                Method = method,
                Status = PaymentStatus.Pending,
                Type = PaymentOrderType.WalletTopUp,
            };

            await _paymentOrderRepository.AddAsync(order, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var gateway = _paymentGatewayFactory.Get(method);

            var url = await gateway.CreatePaymentUrlAsync(order, cancellationToken);

            return OperationResult<string>.Success(url, "Create payment url successfully");
        }

        public async Task<OperationResult<bool>> HandleVnPayCallbackAsync(
            Dictionary<string, string> response,
            CancellationToken cancellationToken
        )
        {
            var gateway = _paymentGatewayFactory.Get(PaymentMethod.Vnpay);

            var valid = gateway.VerifySignature(response);

            if (!valid)
                return OperationResult<bool>.Failure("Invalid signature");

            var orderId = Guid.Parse(response["vnp_TxnRef"]);

            var order = await _paymentOrderRepository.GetByIdAsync(orderId, cancellationToken);

            if (order == null)
                return OperationResult<bool>.Failure("Payment order not found");

            if (order.Status == PaymentStatus.Paid)
                return OperationResult<bool>.Success(true, "Payment already processed");

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

            order.GatewayResponse = System.Text.Json.JsonSerializer.Serialize(response);

            _paymentOrderRepository.Update(order);

            await _walletService.TopUpAsync(
                order.UserId,
                order.FinalAmount,
                order.ExternalTransactionId!,
                cancellationToken
            );

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return OperationResult<bool>.Success(true, "Payment success");
        }
    }
}
