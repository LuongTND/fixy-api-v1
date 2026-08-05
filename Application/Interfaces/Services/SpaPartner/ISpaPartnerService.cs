using Application.Common;
using Application.DTOs.SpaPartner;

namespace Application.Interfaces.Services.SpaPartner
{
    public interface ISpaPartnerService
    {
        // Customer-facing
        Task<OperationResult<PagedResponse<SpaPartnerDto>>> SearchAsync(SearchSpaPartnerQuery query, CancellationToken cancellationToken = default);
        Task<OperationResult<SpaPartnerDetailDto>> GetDetailAsync(Guid id, double? customerLat, double? customerLng, CancellationToken cancellationToken = default);
        Task<OperationResult<List<SpaPartnerDto>>> GetNearbyAsync(double lat, double lng, double radiusKm, int limit, CancellationToken cancellationToken = default);
        Task<OperationResult<SpaPartnerReviewDto>> CreateReviewAsync(Guid spaId, Guid customerProfileId, CreateSpaPartnerReviewDto dto, CancellationToken cancellationToken = default);
        Task<OperationResult<PagedResponse<SpaPartnerReviewDto>>> GetReviewsAsync(Guid spaId, PagedQuery query, CancellationToken cancellationToken = default);

        // Admin
        Task<OperationResult<SpaPartnerDetailDto>> CreateAsync(CreateSpaPartnerDto dto, CancellationToken cancellationToken = default);
        Task<OperationResult<SpaPartnerDetailDto>> UpdateAsync(Guid id, UpdateSpaPartnerDto dto, CancellationToken cancellationToken = default);
        Task<OperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
