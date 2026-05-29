using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class BookingVoucherRepository : Repository<BookingVoucher>, IBookingVoucherRepository
    {
        public BookingVoucherRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<BookingVoucher?> GetByBookingIdAsync(Guid bookingId,CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(bv => bv.Voucher)
                .FirstOrDefaultAsync(bv => bv.BookingId == bookingId, cancellationToken);
        }
    }
}
