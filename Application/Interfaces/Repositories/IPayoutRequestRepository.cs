using Application.Common;
using Domain.Entity;
using Domain.Enum;

namespace Application.Interfaces.Repositories
{
    public interface IPayoutRequestRepository : IRepository<PayoutRequest>
    {
        Task<PayoutRequest?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken);

        Task<List<PayoutRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken);

        Task<List<PayoutRequest>> GetWorkerRequestsAsync(
            Guid workerId,
            CancellationToken cancellationToken
        );

        Task<bool> ExistsPendingRequestAsync(Guid workerId, CancellationToken cancellationToken);
        Task<(List<PayoutRequest>, int)> GetPagedAsync(
            PagedQuery query,
            CancellationToken cancellationToken
        );
        Task<(List<PayoutRequest>, int)> GetWorkerPagedAsync(
            Guid workerId,
            PagedQuery query,
            CancellationToken cancellationToken
        );
    }
}
