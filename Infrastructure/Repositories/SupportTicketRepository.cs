using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class SupportTicketRepository : Repository<SupportTicket>, ISupportTicketRepository
    {
        public SupportTicketRepository(AppDbContext context) : base(context)
        {
        }
    }
}
