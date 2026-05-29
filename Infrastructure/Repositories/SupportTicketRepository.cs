using Application.DTOs.Support;
using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class SupportTicketRepository : Repository<SupportTicket>, ISupportTicketRepository
    {
        public SupportTicketRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<SupportTicket?> GetTicketDetailsWithMessagesAsync(Guid ticketId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.Reporter)
                .Include(x => x.AssignedTo)
                .Include(x => x.Booking)
                    .ThenInclude(b => b.Category)
                .Include(x => x.Messages)
                    .ThenInclude(m => m.Sender)
                        .ThenInclude(s => s.UserRoles)
                            .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(x => x.Id == ticketId, cancellationToken);
        }

        public async Task<(List<SupportTicket> Items, int TotalCount)> GetUserTicketsPagedAsync(
            Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsNoTracking().Where(x => x.ReporterId == userId);
            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
            return (items, totalCount);
        }

        public async Task<(List<SupportTicket> Items, int TotalCount)> GetAdminTicketsPagedAsync(
            AdminTicketsQuery queryDto, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsNoTracking()
                .Include(x => x.Reporter)
                .Include(x => x.AssignedTo)
                .AsQueryable();

            if (queryDto.Status.HasValue)
            {
                query = query.Where(x => x.Status == queryDto.Status.Value);
            }

            if (queryDto.Priority.HasValue)
            {
                query = query.Where(x => x.Priority == queryDto.Priority.Value);
            }

            if (queryDto.ReporterType.HasValue)
            {
                query = query.Where(x => x.ReporterType == queryDto.ReporterType.Value);
            }

            if (!string.IsNullOrWhiteSpace(queryDto.SearchTerm))
            {
                var term = queryDto.SearchTerm.Trim().ToLower();
                query = query.Where(x => x.Subject.ToLower().Contains(term) 
                    || (x.Reporter != null && x.Reporter.FullName.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(queryDto.SortBy))
            {
                switch (queryDto.SortBy.ToLower())
                {
                    case "createddate":
                        query = queryDto.SortDescending ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate);
                        break;
                    case "priority":
                        query = queryDto.SortDescending ? query.OrderByDescending(x => x.Priority) : query.OrderBy(x => x.Priority);
                        break;
                    default:
                        query = query.OrderByDescending(x => x.CreatedDate);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedDate);
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((queryDto.PageNumber - 1) * queryDto.PageSize)
                .Take(queryDto.PageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<(List<SupportMessage> Items, int TotalCount)> GetTicketMessagesPagedAsync(
            Guid ticketId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<SupportMessage>().AsNoTracking()
                .Include(x => x.Sender)
                    .ThenInclude(s => s.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .Where(x => x.TicketId == ticketId);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}

