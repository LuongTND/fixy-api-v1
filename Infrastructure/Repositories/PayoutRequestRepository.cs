using Application.Common;
using Application.Interfaces.Repositories;
using Domain.Entity;
using Domain.Enum;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PayoutRequestRepository : Repository<PayoutRequest>, IPayoutRequestRepository
    {
        public PayoutRequestRepository(AppDbContext context)
            : base(context) { }

        public async Task<PayoutRequest?> GetByIdWithDetailsAsync(
            Guid id,
            CancellationToken cancellationToken
        )
        {
            return await _dbSet
                .Include(x => x.PayoutAccount)
                .Include(x => x.WorkerProfile)
                .Include(x => x.ReviewedBy)
                .Include(x => x.WalletTransactions)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<PayoutRequest>> GetPendingRequestsAsync(
            CancellationToken cancellationToken
        )
        {
            return await _dbSet
                .Include(x => x.WorkerProfile)
                .Include(x => x.PayoutAccount)
                .Where(x => x.Status == PayoutRequestStatus.Pending)
                .OrderBy(x => x.CreatedDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<PayoutRequest>> GetWorkerRequestsAsync(
            Guid workerId,
            CancellationToken cancellationToken
        )
        {
            return await _dbSet
                .Include(x => x.PayoutAccount)
                .Where(x => x.WorkerProfileId == workerId)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsPendingRequestAsync(
            Guid workerId,
            CancellationToken cancellationToken
        )
        {
            return await _dbSet.AnyAsync(
                x => x.WorkerProfileId == workerId && x.Status == PayoutRequestStatus.Pending,
                cancellationToken
            );
        }

        public async Task<(List<PayoutRequest>, int)> GetPagedAsync(
            PagedQuery query,
            CancellationToken cancellationToken
        )
        {
            var data = _dbSet
                .Include(x => x.WorkerProfile)
                .Include(x => x.PayoutAccount)
                .AsQueryable();

            var total = await data.CountAsync(cancellationToken);

            var items = await data.OrderByDescending(x => x.CreatedDate)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return (items, total);
        }

        public async Task<(List<PayoutRequest>, int)> GetWorkerPagedAsync(
            Guid workerId,
            PagedQuery query,
            CancellationToken cancellationToken
        )
        {
            var data = _dbSet
                .Include(x => x.PayoutAccount)
                    .ThenInclude(x => x!.WorkerProfile)
                .Where(x => x.WorkerProfile!.UserId == workerId)
                .AsQueryable();

            var total = await data.CountAsync(cancellationToken);

            var items = await data.OrderByDescending(x => x.CreatedDate)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return (items, total);
        }
    }
}
