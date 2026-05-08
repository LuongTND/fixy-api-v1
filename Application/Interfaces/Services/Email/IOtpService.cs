using Domain.Enum;

namespace Application.Interfaces.Services.Email
{
    public interface IOtpService
    {
        Task SendOtpAsync(string target, EmailPurpose purpose);

        Task VerifyOtpAsync(string target, string otpCode);
    }
}
