using Application.DTOs.Booking;

namespace Application.Interfaces.Hubs
{
    public interface IBookingHubService
    {
        Task SendStatusUpdateAsync(Guid bookingId, BookingStatusUpdateDto dto, CancellationToken cancellationToken = default);
        Task SendLocationUpdateAsync(Guid bookingId, WorkerLocationUpdateDto dto, CancellationToken cancellationToken = default);
    }
}
