using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IMediaRepository : IRepository<Media>
    {
        Task<List<Media>> GetIdentificateImagesByUserId(
            Guid id,
            CancellationToken cancellationToken
        );
        Task<List<Media>> GetPorfolioImagesByUserId(Guid id, CancellationToken cancellationToken);
        Task<List<Media>> GetAllWorkerCertificateImagesByCertificateIds(
            List<Guid> certificateIds,
            CancellationToken cancellationToken
        );
    }
}
