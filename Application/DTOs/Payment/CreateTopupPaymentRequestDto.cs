using Domain.Enum;

namespace Application.DTOs.Payment
{
    public class CreateTopupPaymentRequestDto
    {
        public long Amount { get; set; }

        public PaymentMethod Method { get; set; }
    }
}
