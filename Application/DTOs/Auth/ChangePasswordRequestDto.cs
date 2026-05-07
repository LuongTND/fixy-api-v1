using Domain.Enum;

namespace Application.DTOs.Auth
{
    public class ChangePasswordRequestDto
    {
        public string Target { get; set; } = default!;

        public OtpType Type { get; set; }

        public string NewPassword { get; set; } = default!;
    }
}
