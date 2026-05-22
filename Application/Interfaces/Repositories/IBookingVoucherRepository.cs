using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IBookingVoucherRepository : IRepository<BookingVoucher>
    {
        Task<BookingVoucher?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
    }
}
