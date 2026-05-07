namespace Application.DTOs.Auth
{
    public class LoginRequestDto
    {
        public string Target { get; set; } = default!;

        public string Password { get; set; } = default!;
    }
}
