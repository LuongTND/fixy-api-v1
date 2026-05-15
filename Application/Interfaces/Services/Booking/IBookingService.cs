using Application.Common;
using Application.DTOs.Booking;
using Application.DTOs.Support;

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

        Task<OperationResult> CompleteAsync(Guid bookingId, CompleteBookingRequest request, CancellationToken cancellationToken = default);

        Task<OperationResult<SupportTicketDto>> ReportIssueAsync(Guid bookingId, ReportBookingIssueRequest request, CancellationToken cancellationToken = default);
    }
}
