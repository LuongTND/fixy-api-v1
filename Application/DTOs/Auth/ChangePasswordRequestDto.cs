using Domain.Enum;

namespace Application.DTOs.Auth
{
    public class ChangePasswordRequestDto
    {
        public string Target { get; set; } = default!;
        public string OldPassword { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
    }
}
