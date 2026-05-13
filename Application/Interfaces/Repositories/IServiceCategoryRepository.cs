using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IServiceCategoryRepository : IRepository<ServiceCategory>
    {
        Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> HasChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);
    }
}
