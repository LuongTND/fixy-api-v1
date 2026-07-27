namespace Application.Settings
{
    public class SmsSettings
    {
        public bool UseMock { get; set; } = true;
        public string Provider { get; set; } = "Mock";
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public string SenderId { get; set; } = "FIXY";
        public string BaseUrl { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 10;
    }
}
