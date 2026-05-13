using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class WorkerCertificateRepository
        : Repository<WorkerCertificate>,
            IWorkerCertificateRepository
    {
        public WorkerCertificateRepository(AppDbContext context)
            : base(context) { }
    }
}
