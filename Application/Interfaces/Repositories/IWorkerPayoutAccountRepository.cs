using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IWorkerPayoutAccountRepository : IRepository<WorkerPayoutAccount>
    {
        Task<List<WorkerPayoutAccount>> GetByWorkerIdAsync(
            Guid workerId,
            CancellationToken cancellationToken
        );

        Task<WorkerPayoutAccount?> GetDefaultAsync(
            Guid workerId,
            CancellationToken cancellationToken
        );
    }
}
