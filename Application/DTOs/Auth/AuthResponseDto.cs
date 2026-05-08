namespace Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public Guid UserId { get; set; }
        public string? Email { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
