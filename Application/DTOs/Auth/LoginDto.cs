using Domain.Enum;

namespace Application.DTOs.Auth
{
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? DeviceId { get; set; }
        public string? DeviceName { get; set; }
        public SessionDeviceType? DeviceType { get; set; }
        public string? Os { get; set; }
        public string? AppVersion { get; set; }
    }
}
