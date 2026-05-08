using Application.Common;
using Application.DTOs.Auth;

namespace Application.Interfaces.Services.Auth
{
    public interface IAuthService
    {
        Task<OperationResult> SignupAsync(SignupDto request, string ipAddress, CancellationToken cancellationToken = default);
        Task<OperationResult<AuthResponseDto>> VerifySignupOtpAsync(VerifySignupOtpDto request, string ipAddress, CancellationToken cancellationToken = default);
        Task<OperationResult<AuthResponseDto>> LoginAsync(LoginDto request, string ipAddress, CancellationToken cancellationToken = default);
        Task<OperationResult<AuthResponseDto>> RefreshAsync(RefreshTokenDto request, string ipAddress, CancellationToken cancellationToken = default);
        Task<OperationResult> LogoutAsync(LogoutDto request, string ipAddress, CancellationToken cancellationToken = default);
        Task<OperationResult> RequestPasswordOtpAsync(RequestPasswordOtpDto request, string ipAddress, CancellationToken cancellationToken = default);
        Task<OperationResult> ChangePasswordByOtpAsync(ChangePasswordByOtpDto request, string ipAddress, CancellationToken cancellationToken = default);
    }
}
