namespace Application.Settings
{
    public class OtpSettings
    {
        public int ExpiresMinutes { get; set; } = 3;
        public int MaxAttempts { get; set; } = 3;
        public string Secret { get; set; } = string.Empty;
    }
}
