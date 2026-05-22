using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class WorkerPayoutAccountRepository
        : Repository<WorkerPayoutAccount>,
            IWorkerPayoutAccountRepository
    {
        public WorkerPayoutAccountRepository(AppDbContext context)
            : base(context) { }
    }
}
