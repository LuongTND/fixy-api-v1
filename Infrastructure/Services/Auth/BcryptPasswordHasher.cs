using Application.Interfaces.Services.Auth;

namespace Infrastructure.Services.Auth
{
    public class BcryptPasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 12;

        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password is required", nameof(password));
            }

            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
        }

        public bool VerifyPassword(string hashedPassword, string password)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
