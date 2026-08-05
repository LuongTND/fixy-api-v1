using Application.DTOs.SpaPartner;
using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface ISpaPartnerRepository : IRepository<SpaPartner>
    {
        Task<(List<SpaPartner> Items, Dictionary<Guid, double?> Distances, int TotalCount)> SearchAsync(
            SearchSpaPartnerQuery query,
            CancellationToken cancellationToken = default
        );

        Task<SpaPartner?> GetDetailByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default
        );

        Task<(List<SpaPartner> Items, Dictionary<Guid, double?> Distances)> GetNearbyAsync(
            double lat,
            double lng,
            double radiusKm,
            int limit,
            CancellationToken cancellationToken = default
        );
    }
}
