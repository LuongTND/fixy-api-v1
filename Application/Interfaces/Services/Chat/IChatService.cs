using Application.Common;
using Application.DTOs.Chat;

namespace Application.Interfaces.Services.Chat
{
    public interface IChatService
    {
        Task<OperationResult<PagedResponse<ChatMessageDto>>> GetChatHistoryAsync(Guid bookingId, PagedQuery query, CancellationToken cancellationToken = default);
        Task<OperationResult<ChatMessageDto>> SendMessageAsync(Guid bookingId, SendChatMessageRequest request, CancellationToken cancellationToken = default);
        Task<OperationResult> MarkAsReadAsync(Guid bookingId, CancellationToken cancellationToken = default);
    }
}
