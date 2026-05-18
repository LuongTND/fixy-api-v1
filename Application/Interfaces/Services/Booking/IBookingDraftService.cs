using Application.Common;
using Application.DTOs.BookingDraft;

namespace Application.Interfaces.Services.Booking
{
    public interface IBookingDraftService
    {
        Task<OperationResult<BookingDraftCreatedDto>> CreateAsync(CreateBookingDraftRequest request, CancellationToken cancellationToken = default);
        Task<OperationResult<List<BookingDraftDto>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<OperationResult<BookingDraftDto>> GetByIdAsync(Guid draftId,CancellationToken cancellationToken = default);
        Task<OperationResult> UpdateAsync(Guid draftId,UpdateBookingDraftRequest request,CancellationToken cancellationToken = default);
        Task<OperationResult> DeleteAsync(Guid draftId,CancellationToken cancellationToken = default);
        Task<OperationResult<BookingDraftConfirmedDto>> ConfirmAsync(Guid draftId,CancellationToken cancellationToken = default);
    }
}
