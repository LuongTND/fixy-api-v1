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
        public BookingRepository(AppDbContext context) : base(context) { }

        public async Task<Booking?> GetActiveBookingByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(b => b.WorkerId == workerId && (b.Status == BookingStatus.Traveling || b.Status == BookingStatus.InProgress))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Booking?> GetBookingWithWorkerAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(b => b.Worker)
                .ThenInclude(w => w!.User)
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);
        }

        public async Task<(List<Booking> Items, int TotalCount)> GetWorkerBookingsAsync(
            Guid workerId,
            WorkerBookingsQuery query,
            CancellationToken cancellationToken = default)
        {
            var dbQuery = _dbSet.AsNoTracking()
                .Include(b => b.Worker)
                .ThenInclude(w => w!.User)
                .Where(b => b.WorkerId == workerId);

            if (query.Status.HasValue)
            {
                dbQuery = dbQuery.Where(b => b.Status == query.Status.Value);
            }
            else if (query.IsActive.HasValue)
            {
                var activeStatuses = new[]
                {
                    BookingStatus.Pending,
                    BookingStatus.Confirmed,
                    BookingStatus.Traveling,
                    BookingStatus.Arrived,
                    BookingStatus.InProgress
                };

                if (query.IsActive.Value)
                {
                    dbQuery = dbQuery.Where(b => activeStatuses.Contains(b.Status));
                }
                else
                {
                    dbQuery = dbQuery.Where(b => b.Status == BookingStatus.Completed || b.Status == BookingStatus.Cancelled || b.Status == BookingStatus.Disputed);
                }
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var search = query.SearchTerm.Trim().ToLower();
                dbQuery = dbQuery.Where(b => b.Description.ToLower().Contains(search) || b.Address.ToLower().Contains(search));
            }

            var totalCount = await dbQuery.CountAsync(cancellationToken);

            var items = await dbQuery
                .OrderByDescending(b => b.CreatedDate)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<(List<Booking> Items, int TotalCount)> GetCustomerBookingsAsync(
            Guid customerId,
            CustomerBookingsQuery query,
            CancellationToken cancellationToken = default)
        {
            var dbQuery = _dbSet.AsNoTracking()
                .Include(b => b.Worker)
                .ThenInclude(w => w!.User)
                .Where(b => b.CustomerId == customerId);

            if (query.Status.HasValue)
            {
                dbQuery = dbQuery.Where(b => b.Status == query.Status.Value);
            }
            else if (query.IsActive.HasValue)
            {
                var activeStatuses = new[]
                {
                    BookingStatus.Pending,
                    BookingStatus.PendingPayment,
                    BookingStatus.Matching,
                    BookingStatus.Confirmed,
                    BookingStatus.Traveling,
                    BookingStatus.Arrived,
                    BookingStatus.InProgress
                };

                if (query.IsActive.Value)
                {
                    dbQuery = dbQuery.Where(b => activeStatuses.Contains(b.Status));
                }
                else
                {
                    dbQuery = dbQuery.Where(b => b.Status == BookingStatus.Completed || b.Status == BookingStatus.Cancelled || b.Status == BookingStatus.Disputed);
                }
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var search = query.SearchTerm.Trim().ToLower();
                dbQuery = dbQuery.Where(b => b.Description.ToLower().Contains(search) || b.Address.ToLower().Contains(search));
            }

            var totalCount = await dbQuery.CountAsync(cancellationToken);

            var items = await dbQuery
                .OrderByDescending(b => b.CreatedDate)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
