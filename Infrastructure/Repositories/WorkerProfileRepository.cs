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

        public async Task<(List<WorkerProfile> Items, List<double?> Distances, int TotalCount)> SearchWorkersForCustomerAsync(
            CustomerWorkerSearchQuery query,
            CancellationToken cancellationToken
        )
        {
            // 1. Base query: chỉ thợ đã duyệt, tài khoản hoạt động
            var queryDb = _dbSet
                .Include(x => x.User)
                .Include(x => x.Address)
                .Include(x => x.Services)
                    .ThenInclude(s => s.Category)
                .AsNoTracking()
                .Where(x =>
                    x.Status == Domain.Enum.WorkerStatus.Approved
                    && x.User != null
                    && x.User.IsActive
                    && !x.User.IsDeleted
                );

            // 2. Lọc theo danh mục (hỗ trợ phân cấp bằng Code)
            if (query.CategoryId.HasValue)
            {
                var targetCategory = await _context.Set<ServiceCategory>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == query.CategoryId.Value, cancellationToken);

                if (targetCategory != null)
                {
                    var categoryCode = targetCategory.Code;
                    var codePrefix = categoryCode + ".";

                    queryDb = queryDb.Where(x => x.Services.Any(s =>
                        s.Category != null &&
                        (s.Category.Code == categoryCode || s.Category.Code.StartsWith(codePrefix))
                    ));
                }
            }

            // 3. Tìm kiếm theo từ khóa tự do
            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var keyword = query.SearchTerm.Trim().ToLower();
                queryDb = queryDb.Where(x =>
                    x.User!.FullName.ToLower().Contains(keyword)
                    || (x.Bio != null && x.Bio.ToLower().Contains(keyword))
                    || x.Services.Any(s => s.Category != null && s.Category.Name.ToLower().Contains(keyword))
                );
            }

            // 4. Lọc theo trạng thái Online
            if (query.IsOnline.HasValue)
            {
                queryDb = queryDb.Where(x => x.IsOnline == query.IsOnline.Value);
            }

            // 5. Lọc theo điểm đánh giá
            if (query.MinRating.HasValue)
            {
                queryDb = queryDb.Where(x => x.RatingAvg >= query.MinRating.Value);
            }

            // 6. Lọc theo khu vực hành chính
            if (!string.IsNullOrWhiteSpace(query.City))
            {
                var city = query.City.Trim().ToLower();
                queryDb = queryDb.Where(x => x.Address != null && x.Address.City.ToLower().Contains(city));
            }
            if (!string.IsNullOrWhiteSpace(query.District))
            {
                var district = query.District.Trim().ToLower();
                queryDb = queryDb.Where(x => x.Address != null && x.Address.District.ToLower().Contains(district));
            }
            if (!string.IsNullOrWhiteSpace(query.Ward))
            {
                var ward = query.Ward.Trim().ToLower();
                queryDb = queryDb.Where(x => x.Address != null && x.Address.Ward.ToLower().Contains(ward));
            }

            // 7. Lọc theo khoảng giá
            if (query.MinPrice.HasValue || query.MaxPrice.HasValue)
            {
                queryDb = queryDb.Where(x => x.Services.Any(s =>
                    (!query.CategoryId.HasValue || s.CategoryId == query.CategoryId.Value) &&
                    (query.CategoryId.HasValue || s.IsPrimary) &&
                    (!query.MinPrice.HasValue || s.BasePrice >= query.MinPrice.Value) &&
                    (!query.MaxPrice.HasValue || s.BasePrice <= query.MaxPrice.Value)
                ));
            }

            // 8. Projection với tính toán khoảng cách Haversine
            var hasCoordinates = query.CustomerLat.HasValue && query.CustomerLng.HasValue;
            var lat1Rad = hasCoordinates ? query.CustomerLat!.Value * Math.PI / 180.0 : 0.0;
            var lng1Rad = hasCoordinates ? query.CustomerLng!.Value * Math.PI / 180.0 : 0.0;
            const double EarthRadiusKm = 6371.0;
            const double Deg2Rad = Math.PI / 180.0;

            var projectedQuery = queryDb.Select(x => new
            {
                Worker = x,
                EffectiveLat = x.CurrentLat ?? (x.Address != null ? x.Address.Lat : (double?)null),
                EffectiveLng = x.CurrentLng ?? (x.Address != null ? x.Address.Lng : (double?)null)
            })
            .Select(x => new
            {
                x.Worker,
                DistanceKm = (!hasCoordinates || x.EffectiveLat == null || x.EffectiveLng == null)
                    ? (double?)null
                    : EarthRadiusKm * Math.Acos(
                        (Math.Sin(lat1Rad) * Math.Sin(x.EffectiveLat.Value * Deg2Rad) +
                         Math.Cos(lat1Rad) * Math.Cos(x.EffectiveLat.Value * Deg2Rad) *
                         Math.Cos(x.EffectiveLng.Value * Deg2Rad - lng1Rad)) > 1.0
                            ? 1.0
                            : (Math.Sin(lat1Rad) * Math.Sin(x.EffectiveLat.Value * Deg2Rad) +
                               Math.Cos(lat1Rad) * Math.Cos(x.EffectiveLat.Value * Deg2Rad) *
                               Math.Cos(x.EffectiveLng.Value * Deg2Rad - lng1Rad))
                    )
            });

            // 9. Lọc theo bán kính
            if (hasCoordinates && query.RadiusKm.HasValue)
            {
                projectedQuery = projectedQuery.Where(x =>
                    x.DistanceKm != null && x.DistanceKm <= query.RadiusKm.Value
                );
            }

            // 10. Sắp xếp
            var sortBy = query.SortBy?.ToLower();

            IOrderedQueryable<WorkerSearchProjection> orderedQuery;

            var typedQuery = projectedQuery.Select(x => new WorkerSearchProjection
            {
                Worker = x.Worker,
                DistanceKm = x.DistanceKm
            });

            if (sortBy == "nearest" && hasCoordinates)
            {
                orderedQuery = query.SortDescending
                    ? typedQuery.OrderByDescending(x => x.DistanceKm)
                    : typedQuery.OrderBy(x => x.DistanceKm);
            }
            else if (sortBy == "rating")
            {
                orderedQuery = query.SortDescending
                    ? typedQuery.OrderByDescending(x => x.Worker.RatingAvg)
                    : typedQuery.OrderBy(x => x.Worker.RatingAvg);
            }
            else if (sortBy == "price")
            {
                if (query.SortDescending)
                {
                    orderedQuery = typedQuery.OrderByDescending(x =>
                        x.Worker.Services
                            .Where(s => !query.CategoryId.HasValue || s.CategoryId == query.CategoryId.Value)
                            .Max(s => s.BasePrice));
                }
                else
                {
                    orderedQuery = typedQuery.OrderBy(x =>
                        x.Worker.Services
                            .Where(s => !query.CategoryId.HasValue || s.CategoryId == query.CategoryId.Value)
                            .Min(s => s.BasePrice));
                }
            }
            else if (sortBy == "most_completed")
            {
                orderedQuery = query.SortDescending
                    ? typedQuery.OrderByDescending(x => x.Worker.TotalOrders)
                    : typedQuery.OrderBy(x => x.Worker.TotalOrders);
            }
            else
            {
                // Mặc định: thợ nổi bật (Featured) lên trước, sau đó theo ngày tạo
                orderedQuery = typedQuery
                    .OrderByDescending(x => x.Worker.FeaturedUntil > DateTime.UtcNow)
                    .ThenByDescending(x => x.Worker.CreatedDate);
            }

            var totalCount = await typedQuery.CountAsync(cancellationToken);

            var pagedItems = await orderedQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            var items = pagedItems.Select(x => x.Worker).ToList();
            var distances = pagedItems.Select(x => x.DistanceKm).ToList();

            return (items, distances, totalCount);
        }

        /// <summary>
        /// Helper class for projecting worker search results with calculated distance.
        /// </summary>
        private class WorkerSearchProjection
        {
            public WorkerProfile Worker { get; set; } = null!;
            public double? DistanceKm { get; set; }
        }
    }
}

