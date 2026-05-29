using Application.Common;
using Application.DTOs.Support;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface ISupportTicketService
    {
        Task<OperationResult<SupportTicketDto>> CreateTicketAsync(Guid reporterId, string reporterRole, CreateSupportTicketRequest request, CancellationToken cancellationToken = default);

        Task<OperationResult<PagedResponse<SupportTicketDto>>> GetUserTicketsAsync(Guid userId, PagedQuery query, CancellationToken cancellationToken = default);

        Task<OperationResult<SupportTicketDetailsDto>> GetTicketDetailsAsync(Guid ticketId, Guid userId, string userRole, CancellationToken cancellationToken = default);

        Task<OperationResult<PagedResponse<SupportMessageDto>>> GetTicketMessagesAsync(Guid ticketId, Guid userId, string userRole, PagedQuery query, CancellationToken cancellationToken = default);

        Task<OperationResult<SupportMessageDto>> SendMessageAsync(Guid ticketId, Guid senderId, string senderRole, string content, CancellationToken cancellationToken = default);

        Task<OperationResult<PagedResponse<SupportTicketDto>>> GetAdminTicketsAsync(AdminTicketsQuery query, CancellationToken cancellationToken = default);

        Task<OperationResult> AssignTicketAsync(Guid ticketId, Guid adminId, CancellationToken cancellationToken = default);

        Task<OperationResult> UpdateTicketStatusAsync(Guid ticketId, UpdateTicketStatusRequest request, CancellationToken cancellationToken = default);
    }
}
