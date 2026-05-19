using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IMediaRepository : IRepository<Media>
    {
        Task<Media?> GetAvatarByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<List<Media>> GetIdentificateImagesByUserId(
            Guid userId,
            CancellationToken cancellationToken
        );
        Task<List<Media>> GetPorfolioImagesByUserId(
            Guid userId,
            CancellationToken cancellationToken
        );
        Task<List<Media>> GetAllWorkerCertificateImagesByCertificateIds(
            List<Guid> certificateIds,
            CancellationToken cancellationToken
        );
        Task<List<Media>> GetReviewImagesByReviewIdsAsync(
            List<Guid> reviewIds,
            CancellationToken cancellationToken = default
        );
    }
}
