using Application.DTOs.Support;
using Domain.Entity;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface ISupportTicketRepository : IRepository<SupportTicket>
    {
        Task<SupportTicket?> GetTicketDetailsWithMessagesAsync(Guid ticketId, CancellationToken cancellationToken = default);
        Task<(List<SupportTicket> Items, int TotalCount)> GetUserTicketsPagedAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
        Task<(List<SupportTicket> Items, int TotalCount)> GetAdminTicketsPagedAsync(AdminTicketsQuery query, CancellationToken cancellationToken = default);
        Task<(List<SupportMessage> Items, int TotalCount)> GetTicketMessagesPagedAsync(Guid ticketId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    }
}

