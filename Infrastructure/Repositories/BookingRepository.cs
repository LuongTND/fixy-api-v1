using Application.DTOs.Booking;
using Application.Interfaces.Repositories;
using Domain.Entity;
using Domain.Enum;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(AppDbContext context)
            : base(context) { }

        public async Task<Booking?> GetDetailByIdAsync(
            Guid bookingId,
            CancellationToken ct = default
        )
        {
            return await _dbSet
                .Include(x => x.CustomerProfile)
                    .ThenInclude(x => x!.User)
                .Include(x => x.WorkerProfile)
                    .ThenInclude(x => x!.User)
                .FirstOrDefaultAsync(x => x.Id == bookingId, ct);
        }

        public async Task<Booking?> GetActiveBookingByWorkerProfileIdAsync(
            Guid workerProfileId,
            CancellationToken ct = default
        )
        {
            return await _dbSet.FirstOrDefaultAsync(
                x =>
                    x.WorkerProfileId == workerProfileId
                    && (
                        x.Status == BookingStatus.Traveling || x.Status == BookingStatus.InProgress
                    ),
                ct
            );
        }

        public async Task<Booking?> GetBookingWithWorkerAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet
                .Include(x => x.WorkerProfile)
                    .ThenInclude(x => x!.User)
                .Include(x => x.CustomerProfile)
                    .ThenInclude(x => x!.User)
                .Include(x => x.Category)
                .Include(x => x.PaymentOrder)
                .FirstOrDefaultAsync(x => x.Id == bookingId, cancellationToken);
        }

        public async Task<(List<Booking>, int)> GetWorkerBookingsAsync(
            Guid workerProfileId,
            WorkerBookingsQuery query,
            CancellationToken ct = default
        )
        {
            var dbQuery = _dbSet
                .AsNoTracking()
                .Include(x => x.CustomerProfile)
                    .ThenInclude(x => x!.User)
                .Include(x => x.WorkerProfile)
                    .ThenInclude(x => x!.User)
                .Where(x => x.WorkerProfileId == workerProfileId);

            if (query.Status.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.Status == query.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var search = query.SearchTerm.Trim().ToLower();

                dbQuery = dbQuery.Where(x =>
                    x.Description.ToLower().Contains(search) || x.Address.ToLower().Contains(search)
                );
            }

            var totalCount = await dbQuery.CountAsync(ct);

            var items = await dbQuery
                .OrderByDescending(x => x.CreatedDate)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<(List<Booking>, int)> GetCustomerBookingsAsync(
            Guid customerProfileId,
            CustomerBookingsQuery query,
            CancellationToken ct = default
        )
        {
            var dbQuery = _dbSet
                .AsNoTracking()
                .Include(x => x.CustomerProfile)
                    .ThenInclude(x => x!.User)
                .Include(x => x.WorkerProfile)
                    .ThenInclude(x => x!.User)
                .Where(x => x.CustomerProfileId == customerProfileId);

            if (query.Status.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.Status == query.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var search = query.SearchTerm.Trim().ToLower();

                dbQuery = dbQuery.Where(x =>
                    x.Description.ToLower().Contains(search) || x.Address.ToLower().Contains(search)
                );
            }

            var totalCount = await dbQuery.CountAsync(ct);

            var items = await dbQuery
                .OrderByDescending(x => x.CreatedDate)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<(List<Booking> Items, int TotalCount)> GetAllBookingsAsync(
            AllBookingsQuery query,
            CancellationToken ct = default
        )
        {
            var dbQuery = _dbSet
                .AsNoTracking()
                .Include(x => x.CustomerProfile)
                    .ThenInclude(x => x!.User)
                .Include(x => x.WorkerProfile)
                    .ThenInclude(x => x!.User)
                .AsQueryable();

            if (query.Status.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.Status == query.Status.Value);
            }

            if (query.CustomerProfileId.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.CustomerProfileId == query.CustomerProfileId.Value);
            }

            if (query.WorkerProfileId.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.WorkerProfileId == query.WorkerProfileId.Value);
            }

            if (query.FromDate.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.CreatedDate >= query.FromDate.Value);
            }

            if (query.ToDate.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.CreatedDate <= query.ToDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var search = query.SearchTerm.Trim().ToLower();
                dbQuery = dbQuery.Where(x =>
                    x.Description.ToLower().Contains(search) || x.Address.ToLower().Contains(search)
                );
            }

            var totalCount = await dbQuery.CountAsync(ct);

            var items = await dbQuery
                .OrderByDescending(x => x.CreatedDate)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<BookingAdminStatsDto> GetAdminStatsAsync(
            AllBookingsQuery query,
            CancellationToken ct = default
        )
        {
            var dbQuery = _dbSet.AsNoTracking().AsQueryable();

            if (query.Status.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.Status == query.Status.Value);
            }

            if (query.CustomerProfileId.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.CustomerProfileId == query.CustomerProfileId.Value);
            }

            if (query.WorkerProfileId.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.WorkerProfileId == query.WorkerProfileId.Value);
            }

            if (query.FromDate.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.CreatedDate >= query.FromDate.Value);
            }

            if (query.ToDate.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.CreatedDate <= query.ToDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var search = query.SearchTerm.Trim().ToLower();
                dbQuery = dbQuery.Where(x =>
                    x.Description.ToLower().Contains(search) || x.Address.ToLower().Contains(search)
                );
            }

            var allStats = await dbQuery
                .GroupBy(x => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    InProgress = g.Count(x => x.Status != BookingStatus.Completed),
                    Completed = g.Count(x => x.Status == BookingStatus.Completed),
                    TotalValue = g.Sum(x => x.FinalPrice ?? x.EstimatedPrice) ?? 0
                })
                .FirstOrDefaultAsync(ct);

            return new BookingAdminStatsDto
            {
                TotalBookings = allStats?.Total ?? 0,
                InProgressBookings = allStats?.InProgress ?? 0,
                CompletedBookings = allStats?.Completed ?? 0,
                TotalValue = allStats?.TotalValue ?? 0
            };
        }

        public async Task LoadWorkerAndPaymentOrderAsync(
            Booking booking,
            CancellationToken cancellationToken = default
        )
        {
            await _context.Entry(booking).Reference(b => b.WorkerProfile).LoadAsync(cancellationToken);
            await _context.Entry(booking).Reference(b => b.PaymentOrder).LoadAsync(cancellationToken);
        }
    }
}
