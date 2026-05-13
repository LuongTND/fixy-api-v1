using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class WorkerServiceRepository : Repository<WorkerService>, IWorkerServiceRepository
    {
        public WorkerServiceRepository(AppDbContext context)
            : base(context) { }
    }
}
