using Domain.Enum;

namespace Application.DTOs.Auth
{
    public class VerifyOtpRequestDto
    {
        public string Target { get; set; } = default!;

        public OtpType Type { get; set; }

        public string OtpCode { get; set; } = default!;
    }
}
