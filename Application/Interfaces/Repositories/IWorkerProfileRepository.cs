using Application.Common;
using Application.DTOs.WorkerProfile;
using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IWorkerProfileRepository : IRepository<WorkerProfile>
    {
        Task<(List<WorkerProfile>, int)> GetWorkerProfilesAsync(
            WorkerProfileQuery query,
            CancellationToken cancellationToken
        );
        Task<WorkerProfile?> GetWorkerProfileDetailByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default
        );
    }
}
