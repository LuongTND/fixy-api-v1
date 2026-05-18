using Domain.Entity;
using Domain.Enum;

namespace Application.Interfaces.Repositories
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<Booking?> GetActiveBookingByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default);
        Task<Booking?> GetBookingWithWorkerAsync(Guid bookingId, CancellationToken cancellationToken = default);
    }
}
