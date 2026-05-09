namespace Application.DTOs.Auth
{
    public class SignupDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }
}
