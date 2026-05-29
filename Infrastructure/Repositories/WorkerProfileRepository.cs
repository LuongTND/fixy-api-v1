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
                .Include(x => x.Address)
                .Include(x => x.Services)
                    .ThenInclude(s => s.Category)
                .AsNoTracking();

            // Filter category
            if (query.CategoryId != null)
            {
                queryDb = queryDb.Where(x => x.Services.Any(s => s.CategoryId == query.CategoryId));
            }

            // Filter status
            if (query.Status != null)
            {
                queryDb = queryDb.Where(x => x.Status == query.Status);
            }

            // Search
            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var keyword = query.SearchTerm.Trim().ToLower();

                queryDb = queryDb.Where(x =>
                    (x.Bio != null && x.Bio.ToLower().Contains(keyword))
                    || (x.User != null && x.User.FullName.ToLower().Contains(keyword))
                );
            }

            // Sort
            queryDb = query.SortBy?.ToLower() switch
            {
                "name" => query.SortDescending
                    ? queryDb.OrderByDescending(x => x.User!.FullName)
                    : queryDb.OrderBy(x => x.User!.FullName),

                "yearexperiences" => query.SortDescending
                    ? queryDb.OrderByDescending(x => x.ExperienceYears)
                    : queryDb.OrderBy(x => x.ExperienceYears),

                "rating" => query.SortDescending
                    ? queryDb.OrderByDescending(x => x.RatingAvg)
                    : queryDb.OrderBy(x => x.RatingAvg),

                "totalreviews" => query.SortDescending
                    ? queryDb.OrderByDescending(x => x.TotalReviews)
                    : queryDb.OrderBy(x => x.TotalReviews),

                "createddate" => query.SortDescending
                    ? queryDb.OrderByDescending(x => x.CreatedDate)
                    : queryDb.OrderBy(x => x.CreatedDate),

                _ => queryDb.OrderByDescending(x => x.CreatedDate),
            };

            var totalCount = await queryDb.CountAsync(cancellationToken);

            var items = await queryDb
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<WorkerProfile?> GetWorkerProfileByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet
                .Include(x => x.User)
                .Include(x => x.Address)
                .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        }

        public async Task<WorkerProfile?> GetWorkerProfileDetailByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet
                .Include(x => x.User)
                // Worker Address
                .Include(x => x.Address)
                // Certificates
                .Include(x => x.Certificates)
                // Services
                .Include(x => x.Services)
                    .ThenInclude(s => s.Category)
                // Reviews
                .Include(x => x.Reviews)
                // Schedule
                .Include(x => x.WeeklySchedules)
                .Include(x => x.ScheduleExceptions)
                .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        }

        public async Task<WorkerProfile?> GetDetailByIdAsync(
            Guid workerProfileId,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet
                .Include(x => x.User)
                .Include(x => x.Address)
                .Include(x => x.Certificates)
                .Include(x => x.Services)
                    .ThenInclude(s => s.Category)
                .Include(x => x.Reviews)
                .FirstOrDefaultAsync(x => x.Id == workerProfileId, cancellationToken);
        }

        public async Task<bool> IsApprovedWorkerAsync(
            Guid workerProfileId,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet.AnyAsync(
                x => x.Id == workerProfileId && x.Status == Domain.Enum.WorkerStatus.Approved,
                cancellationToken
            );
        }
    }
}
