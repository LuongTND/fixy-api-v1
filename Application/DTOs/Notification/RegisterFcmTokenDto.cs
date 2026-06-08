namespace Application.DTOs.Notification
{
    public class RegisterFcmTokenDto
    {
        public string Token { get; set; } = string.Empty;
        public string? DeviceType { get; set; }
        public string? Browser { get; set; }
    }
}
