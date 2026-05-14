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

        public override async Task<List<ServiceCategory>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var categories = await _dbSet.ToListAsync(cancellationToken);

            return categories
                .OrderBy(x => x.Code.Split('.').Length) 
                .ThenBy(x => x.ParentId)                 
                .ThenBy(x => x.SortOrder)                
                .ToList();
        }

        public async Task<bool> ExistsByIdAsync(Guid id,CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<bool> HasChildrenAsync(Guid parentId,CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(x => x.ParentId == parentId, cancellationToken);
        }

        public async Task<string?> GetLastSiblingCodeAsync(Guid? parentId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(x => x.ParentId == parentId)
                .OrderByDescending(x => x.Code)
                .Select(x => x.Code)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
