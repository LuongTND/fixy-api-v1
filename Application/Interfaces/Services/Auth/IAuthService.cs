using Application.DTOs.Auth;

namespace Application.Interfaces.Services.Auth
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(
            RegisterRequestDto request,
            CancellationToken cancellationToken
        );

        Task<AuthResponseDto> LoginAsync(
            LoginRequestDto request,
            CancellationToken cancellationToken
        );
        Task<AuthResponseDto> GoogleLoginAsync(
            GoogleLoginRequestDto requestDto,
            CancellationToken cancellationToken
        );
        Task<AuthResponseDto> RefreshTokenAsync(
            string refreshToken,
            CancellationToken cancellationToken
        );
        Task ChangePasswordAsync(
            ChangePasswordRequestDto request,
            CancellationToken cancellationToken
        );
        Task ResetPasswordAsync(
            ResetPasswordRequestDto request,
            CancellationToken cancellationToken
        );
    }
}
