using Domain.Enum;

namespace Application.DTOs.Auth
{
    public class VerifyOtpRequestDto
    {
        public string Target { get; set; } = default!;
        public string OtpCode { get; set; } = default!;
    }
}
