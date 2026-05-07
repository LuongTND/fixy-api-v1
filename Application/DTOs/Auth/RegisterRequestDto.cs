using Domain.Enum;

namespace Application.DTOs.Auth
{
    public class RegisterRequestDto
    {
        public string FullName { get; set; } = default!;

        public string Password { get; set; } = default!;

        public string Target { get; set; } = default!;

        // phone OR email

        public OtpType Type { get; set; }
    }
}
