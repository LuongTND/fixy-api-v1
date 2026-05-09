using Domain.Enum;

namespace Application.Interfaces.Services.Auth
{
    public interface IOtpService
    {
        Task<string> CreateOtpAsync(Guid userId, UserOtpType type, string? ipAddress, CancellationToken cancellationToken = default);
        Task<bool> VerifyOtpAsync(Guid userId, UserOtpType type, string otp, CancellationToken cancellationToken = default);
    }
}
