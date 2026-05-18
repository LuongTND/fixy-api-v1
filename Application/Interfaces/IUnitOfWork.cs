using Microsoft.EntityFrameworkCore.Storage;

namespace Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        int SaveChanges();
        Task<IDbContextTransaction> BeginTransactionAsync(
            CancellationToken cancellationToken = default
        );

        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
