using Domain.Enum;

namespace Application.DTOs.Auth
{
    public class ForgotPasswordRequestDto
    {
        public string Target { get; set; } = default!;
    }
}
