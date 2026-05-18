using Application.DTOs.WorkerProfile;
using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class WorkerProfileRepository : Repository<WorkerProfile>, IWorkerProfileRepository
    {
        public WorkerProfileRepository(AppDbContext context)
            : base(context) { }

        public async Task<(List<WorkerProfile>, int)> GetWorkerProfilesAsync(
            WorkerProfileQuery query,
            CancellationToken cancellationToken
        )
        {
            var queryDb = _dbSet
                .Include(x => x.User)
                .Include(x => x.Services)
                    .ThenInclude(s => s.Category)
                .AsNoTracking();
            if (query.CategoryId != null)
            {
                queryDb = queryDb.Where(x => x.Services.Any(s => s.CategoryId == query.CategoryId));
            }
            if (query.Status != null)
            {
                queryDb = queryDb.Where(x => x.Status == query.Status);
            }
            //Search
            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var keyword = query.SearchTerm.Trim().ToLower();

                queryDb = queryDb.Where(x => x.Bio != null && x.Bio.ToLower().Contains(keyword));
            }
            //Sort
            queryDb = query.SortBy?.ToLower() switch
            {
                "name" => query.SortDescending
                    ? queryDb.OrderByDescending(x => x.User!.FullName)
                    : queryDb.OrderBy(x => x.User!.FullName),
                "yearExperiences" => query.SortDescending
                    ? queryDb.OrderByDescending(x => x.ExperienceYears)
                    : queryDb.OrderBy(x => x.ExperienceYears),

                "createddate" => query.SortDescending
                    ? queryDb.OrderByDescending(x => x.CreatedDate)
                    : queryDb.OrderBy(x => x.CreatedDate),

                _ => queryDb.OrderByDescending(x => x.CreatedDate), // default
            };
            var totalCount = await queryDb.CountAsync(cancellationToken);

            var items = await queryDb
                .OrderByDescending(x => x.CreatedDate)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<WorkerProfile?> GetWorkerProfileDetailByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet
                .Include(x => x.User)
                .Include(x => x.Certificates)
                .Include(x => x.Services)
                    .ThenInclude(s => s.Category)
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }
    }
}
