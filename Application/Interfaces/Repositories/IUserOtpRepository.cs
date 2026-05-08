using Domain.Entity;
using Domain.Enum;

namespace Application.Interfaces.Repositories
{
    public interface IUserOtpRepository : IRepository<UserOtp>
    {
        Task<List<UserOtp>> GetActiveOtpsAsync(Guid userId, UserOtpType type, CancellationToken cancellationToken = default);
        Task<UserOtp?> GetLatestActiveOtpAsync(Guid userId, UserOtpType type, CancellationToken cancellationToken = default);
    }
}
