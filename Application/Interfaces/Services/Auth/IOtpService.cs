using Domain.Enum;

namespace Application.Interfaces.Services.Auth
{
    public interface IOtpService
    {
        Task SendOtpAsync(string target, OtpType type);

        Task VerifyOtpAsync(string target, OtpType type, string otpCode);
    }
}
