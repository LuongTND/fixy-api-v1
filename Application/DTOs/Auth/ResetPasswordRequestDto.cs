using Domain.Enum;

namespace Application.DTOs.Auth
{
    public class ResetPasswordRequestDto
    {
        public string Target { get; set; } = default!;
        public string Otp { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
    }
}
