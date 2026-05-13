using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IWorkerServiceRepository : IRepository<WorkerService>
    {
        Task<(long? MinPrice, long? MaxPrice)> GetPriceRangeAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default
        );
    }
}
