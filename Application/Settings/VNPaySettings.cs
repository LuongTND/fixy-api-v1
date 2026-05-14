namespace Application.Settings
{
    public class VNPaySettings
    {
        public string TmnCode { get; set; } = default!;
        public string HashSecret { get; set; } = default!;
        public string BaseUrl { get; set; } = default!;
        public string ReturnUrl { get; set; } = default!;
    }
}
