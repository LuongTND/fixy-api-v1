using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IVoucherRepository : IRepository<Voucher>
    {
        Task<Voucher?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<bool> IsCodeExistsAsync(string code, CancellationToken cancellationToken = default);
        Task<(List<Voucher> Items, int TotalCount)> GetPagedAsync(
            string? searchTerm,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default
        );
    }
}
