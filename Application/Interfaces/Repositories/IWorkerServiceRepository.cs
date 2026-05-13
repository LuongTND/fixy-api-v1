namespace Application.Interfaces.Repositories
{
    public interface IWorkerServiceRepository
    {
        Task<(long? MinPrice, long? MaxPrice)> GetPriceRangeAsync(Guid categoryId,CancellationToken cancellationToken = default);
    }
}
