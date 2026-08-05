using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface ISpaServiceCategoryRepository : IRepository<SpaServiceCategory>
    {
        Task<List<SpaServiceCategory>> GetActiveCategoriesWithSpaCountAsync(CancellationToken cancellationToken = default);
    }
}
