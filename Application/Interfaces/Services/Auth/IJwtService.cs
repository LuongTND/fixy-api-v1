using Domain.Entity;

namespace Application.Interfaces.Services.Auth
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user, IList<string> roles);

        string GenerateRefreshToken();
    }
}
