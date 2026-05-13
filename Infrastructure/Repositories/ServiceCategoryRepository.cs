using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ServiceCategoryRepository : Repository<ServiceCategory>, IServiceCategoryRepository
    {
        public ServiceCategoryRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistsByIdAsync(Guid id,CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<bool> HasChildrenAsync(Guid parentId,CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(x => x.ParentId == parentId, cancellationToken);
        }
    }
}
