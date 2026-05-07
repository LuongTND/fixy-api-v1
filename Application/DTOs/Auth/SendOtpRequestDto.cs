using Domain.Enum;

namespace Application.DTOs.Auth
{
    public class SendOtpRequestDto
    {
        public string Target { get; set; } = default!;

        public OtpType Type { get; set; }
    }
}
