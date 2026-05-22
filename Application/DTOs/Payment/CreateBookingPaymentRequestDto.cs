using Domain.Enum;

namespace Application.DTOs.Payment
{
    public class CreateBookingPaymentRequestDto
    {
        public PaymentMethod Method { get; set; }
    }
}
