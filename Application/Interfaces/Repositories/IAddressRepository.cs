using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IAddressRepository : IRepository<Address>
    {
        Task<List<Address>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

        Task<Address?> GetByIdAndUserAsync(
            Guid addressId,
            Guid userId,
            CancellationToken ct = default
        );

        Task<List<Address>> GetDefaultByUserIdAsync(Guid userId, CancellationToken ct = default);
    }
}
