using Application.Common;
using Domain.Enum;

namespace Application.Interfaces.Services.Email
{
    public interface IOtpService
    {
        Task<OperationResult> SendOtpAsync(string target, EmailPurpose purpose);

        Task<OperationResult> VerifyOtpAsync(string target, string otpCode);
    }
}
