using Application.DTOs.SpaPartner;
using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class SpaPartnerRepository : Repository<SpaPartner>, ISpaPartnerRepository
    {
        public SpaPartnerRepository(AppDbContext context)
            : base(context) { }

        public async Task<(List<SpaPartner> Items, Dictionary<Guid, double?> Distances, int TotalCount)> SearchAsync(
            SearchSpaPartnerQuery query,
            CancellationToken cancellationToken = default
        )
        {
            var dbQuery = _dbSet
                .Include(x => x.Promotions)
                .Include(x => x.Services)
                    .ThenInclude(s => s.SpaServiceCategory)
                .Where(x => x.IsActive)
                .AsNoTracking();

            // Filter by SpaServiceCategoryId
            if (query.SpaServiceCategoryId.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.Services.Any(s => s.SpaServiceCategoryId == query.SpaServiceCategoryId.Value && s.IsActive));
            }

            // Filter by City
            if (!string.IsNullOrWhiteSpace(query.City))
            {
                var cityKeyword = query.City.Trim().ToLower();
                dbQuery = dbQuery.Where(x => x.City != null && x.City.ToLower().Contains(cityKeyword));
            }

            // Filter by SearchTerm (Name, Address, Description)
            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var keyword = query.SearchTerm.Trim().ToLower();
                dbQuery = dbQuery.Where(x =>
                    x.Name.ToLower().Contains(keyword) ||
                    x.Address.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword))
                );
            }

            // Filter by MinRating
            if (query.MinRating.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.RatingAvg >= query.MinRating.Value);
            }

            // Filter by HasPromotion
            if (query.HasPromotion == true)
            {
                var now = DateTime.UtcNow;
                dbQuery = dbQuery.Where(x => x.Promotions.Any(p => p.IsActive && p.StartsAt <= now && p.ExpiresAt >= now));
            }

            var allItems = await dbQuery.ToListAsync(cancellationToken);

            // Compute distances if customer location provided
            var distances = new Dictionary<Guid, double?>();
            if (query.CustomerLat.HasValue && query.CustomerLng.HasValue)
            {
                foreach (var item in allItems)
                {
                    if (item.Lat.HasValue && item.Lng.HasValue)
                    {
                        var dist = CalculateHaversineDistance(
                            query.CustomerLat.Value,
                            query.CustomerLng.Value,
                            item.Lat.Value,
                            item.Lng.Value
                        );
                        distances[item.Id] = Math.Round(dist, 1);
                    }
                    else
                    {
                        distances[item.Id] = null;
                    }
                }

                // Filter by MaxDistanceKm if specified
                if (query.MaxDistanceKm.HasValue)
                {
                    allItems = allItems
                        .Where(x => distances[x.Id].HasValue && distances[x.Id]!.Value <= query.MaxDistanceKm.Value)
                        .ToList();
                }
            }

            // Sorting
            IEnumerable<SpaPartner> sorted = allItems;
            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                switch (query.SortBy.ToLower())
                {
                    case "distance":
                        if (query.CustomerLat.HasValue && query.CustomerLng.HasValue)
                        {
                            sorted = query.SortDescending
                                ? allItems.OrderByDescending(x => distances[x.Id] ?? double.MaxValue)
                                : allItems.OrderBy(x => distances[x.Id] ?? double.MaxValue);
                        }
                        break;
                    case "rating":
                        sorted = query.SortDescending
                            ? allItems.OrderByDescending(x => x.RatingAvg)
                            : allItems.OrderBy(x => x.RatingAvg);
                        break;
                    case "name":
                        sorted = query.SortDescending
                            ? allItems.OrderByDescending(x => x.Name)
                            : allItems.OrderBy(x => x.Name);
                        break;
                    default:
                        sorted = allItems.OrderBy(x => x.SortOrder).ThenByDescending(x => x.RatingAvg);
                        break;
                }
            }
            else
            {
                sorted = allItems.OrderBy(x => x.SortOrder).ThenByDescending(x => x.RatingAvg);
            }

            var totalCount = sorted.Count();
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 10 : query.PageSize;

            var pagedItems = sorted
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (pagedItems, distances, totalCount);
        }

        public async Task<SpaPartner?> GetDetailByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet
                .Include(x => x.Promotions)
                .Include(x => x.Services)
                    .ThenInclude(s => s.SpaServiceCategory)
                .Include(x => x.Gallery)
                .Include(x => x.Reviews.Where(r => r.IsVisible))
                    .ThenInclude(r => r.CustomerProfile)
                        .ThenInclude(c => c!.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, cancellationToken);
        }

        public async Task<(List<SpaPartner> Items, Dictionary<Guid, double?> Distances)> GetNearbyAsync(
            double lat,
            double lng,
            double radiusKm,
            int limit,
            CancellationToken cancellationToken = default
        )
        {
            var allItems = await _dbSet
                .Include(x => x.Promotions)
                .Include(x => x.Services)
                .Where(x => x.IsActive && x.Lat.HasValue && x.Lng.HasValue)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var distances = new Dictionary<Guid, double?>();
            var nearbyItems = new List<SpaPartner>();

            foreach (var item in allItems)
            {
                var dist = CalculateHaversineDistance(lat, lng, item.Lat!.Value, item.Lng!.Value);
                if (dist <= radiusKm)
                {
                    distances[item.Id] = Math.Round(dist, 1);
                    nearbyItems.Add(item);
                }
            }

            var result = nearbyItems
                .OrderBy(x => distances[x.Id])
                .Take(limit)
                .ToList();

            return (result, distances);
        }

        private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth radius in km
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRadians(double angle) => (Math.PI / 180) * angle;
    }
}
