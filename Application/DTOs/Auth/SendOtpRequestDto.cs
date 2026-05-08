using Domain.Enum;

namespace Application.DTOs.Auth
{
    public class SendOtpRequestDto
    {
        public string Target { get; set; } = default!;
        public EmailPurpose Purpose { get; set; } = default!;
    }
}
