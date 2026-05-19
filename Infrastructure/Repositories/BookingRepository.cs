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
    }
}
