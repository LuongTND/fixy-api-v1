using Application.DTOs.Booking;
using Domain.Entity;
using Domain.Enum;

namespace Application.Interfaces.Repositories
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<Booking?> GetDetailByIdAsync(Guid bookingId, CancellationToken ct = default);
        Task<Booking?> GetBookingWithWorkerAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default
        );
        Task<Booking?> GetActiveBookingByWorkerProfileIdAsync(
            Guid workerProfileId,
            CancellationToken ct = default
        );
        Task<(List<Booking> Items, int TotalCount)> GetWorkerBookingsAsync(
            Guid workerId,
            WorkerBookingsQuery query,
            CancellationToken cancellationToken = default
        );
        Task<(List<Booking> Items, int TotalCount)> GetCustomerBookingsAsync(
            Guid customerId,
            CustomerBookingsQuery query,
            CancellationToken cancellationToken = default
        );

        Task<(List<Booking> Items, int TotalCount)> GetAllBookingsAsync(
            AllBookingsQuery query,
            CancellationToken cancellationToken = default
        );

        Task<BookingAdminStatsDto> GetAdminStatsAsync(
            AllBookingsQuery query,
            CancellationToken cancellationToken = default
        );

        Task LoadWorkerAndPaymentOrderAsync(
            Booking booking,
            CancellationToken cancellationToken = default
        );
    }
}
