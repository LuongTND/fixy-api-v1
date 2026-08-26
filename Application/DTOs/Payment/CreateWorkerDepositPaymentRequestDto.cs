using Domain.Enum;

namespace Application.DTOs.Payment
{
    public class CreateWorkerDepositPaymentRequestDto
    {
        public PaymentMethod Method { get; set; }
    }
}
