using Application.Common;
using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IWorkerProfileRepository : IRepository<WorkerProfile>
    {
        Task<(List<WorkerProfile> Items, int TotalCount)> GetPagedWorkerRegisterRequestAsync(
            PagedQuery query,
            CancellationToken cancellationToken = default
        );
        Task<WorkerProfile?> GetWorkerProfileDetailByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default
        );
    }
}
