using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IAddressRepository : IRepository<Address>
    {
        Task<List<Address>> GetByCustomerProfileIdAsync(
            Guid customerProfileId,
            CancellationToken ct = default
        );

        Task<Address?> GetWorkerAddressAsync(Guid workerProfileId, CancellationToken ct = default);

        Task<Address?> GetDefaultByCustomerProfileIdAsync(
            Guid customerProfileId,
            CancellationToken ct = default
        );
    }
}
