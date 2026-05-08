using Domain.Entity;

namespace Application.Interfaces.Services.Auth
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}
