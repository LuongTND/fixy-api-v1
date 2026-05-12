namespace Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string UserId { get; set; } = default!;
        public string Email { get; set; } = default!;
        public IEnumerable<string> Roles { get; set; } = new List<string>();
        public string AccessToken { get; set; } = default!;

        public string RefreshToken { get; set; } = default!;
    }
}
