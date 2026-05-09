using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class UserSessionRepository : Repository<UserSession>, IUserSessionRepository
    {
        public UserSessionRepository(AppDbContext context) : base(context)
        {
        }
    }
}
