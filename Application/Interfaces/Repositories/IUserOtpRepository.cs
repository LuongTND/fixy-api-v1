using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IUserOtpRepository : IRepository<UserOtp>
    {
        Task<UserOtp?> GetLatestByTargetAsync(string target, CancellationToken ct = default);

        Task<UserOtp?> GetVerifiedOtpAsync(string target, CancellationToken ct = default);
        Task<List<UserOtp>> GetUnusedOtpsAsync(string target, CancellationToken ct = default);

        Task<UserOtp?> GetLatestOtpAsync(
            string target,
            string otpCode,
            CancellationToken ct = default
        );
        Task<UserOtp?> GetLatestOtpByTargetAsync(string target, CancellationToken ct);
    }
}
