using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IWorkerServiceRepository : IRepository<WorkerService>
    {
        Task<WorkerService?> GetByWorkerAndCategoryWithOptionsAsync(
            Guid workerProfileId,
            Guid categoryId,
            CancellationToken cancellationToken = default
        );

        Task<WorkerService?> GetByCategoryWithOptionsAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default
        );

        Task<(long? MinPrice, long? MaxPrice)> GetPriceRangeAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default
        );
    }
}

