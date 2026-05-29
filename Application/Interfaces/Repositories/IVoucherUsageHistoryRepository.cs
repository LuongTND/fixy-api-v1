using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IVoucherUsageHistoryRepository : IRepository<VoucherUsageHistory>
    {
        Task<int> GetUsageCountByUserAsync(Guid voucherId, Guid userId, CancellationToken cancellationToken = default);
        Task<List<VoucherUsageHistory>> GetByVoucherIdAsync(Guid voucherId, CancellationToken cancellationToken = default);
    }
}
