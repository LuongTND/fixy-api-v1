using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface ICustomerProfileRepository : IRepository<CustomerProfile>
    {
        Task<CustomerProfile?> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default
        );
    }
}
