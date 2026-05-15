using Application.Common;
using Application.DTOs.Booking;

namespace Application.Interfaces.Services.Booking
{
    public interface IBookingService
    {
        Task<OperationResult<BookingDetailDto>> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken = default);

        // Pending --> Confirmed
        Task<OperationResult> AcceptAsync(Guid bookingId, CancellationToken cancellationToken = default);

        // Confirmed --> Traveling
        Task<OperationResult> StartTravelAsync(Guid bookingId, CancellationToken cancellationToken = default);


        // Status: Traveling -> Arrived.
        Task<OperationResult> ArriveAsync(Guid bookingId, CancellationToken cancellationToken = default);

        // Status: Arrived -> InProgress.
        Task<OperationResult> StartWorkAsync(Guid bookingId, CancellationToken cancellationToken = default);

        // Status: InProgress -> Completed.
        Task<OperationResult> CompleteAsync(Guid bookingId, CompleteBookingRequest request, CancellationToken cancellationToken = default);
    }
}
