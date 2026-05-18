using Application.Common;
using Application.DTOs.Booking;
using Application.DTOs.Worker;

namespace Application.Interfaces.Services.Worker
{

    public interface IWorkerLocationService
    {
        Task<OperationResult> UpdateLocationAsync(Guid userId, UpdateWorkerLocationRequest request, CancellationToken cancellationToken = default);
        Task<WorkerLocationUpdateDto?> GetLastLocationAsync(Guid workerId, CancellationToken cancellationToken = default);
        Task<OperationResult<BookingTrackingDto>> GetBookingTrackingAsync(Guid bookingId, CancellationToken cancellationToken = default);
    }
}
