using Application.Interfaces.Repositories;
using Domain.Entity;
using Domain.Enum;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;

namespace Infrastructure.Repositories
{
    public class MediaRepository : Repository<Media>, IMediaRepository
    {
        public MediaRepository(AppDbContext context)
            : base(context) { }

        public async Task<List<Media>> GetAllWorkerCertificateImagesByCertificateIds(
            List<Guid> certificateIds,
            CancellationToken cancellationToken
        )
        {
            return await _dbSet
                .Where(x =>
                    x.OwnerType == MediaOwnerType.Certificate
                    && x.Category == MediaCategory.Certificate
                    && certificateIds.Contains(x.OwnerId)
                )
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Media>> GetIdentificateImagesByUserId(
            Guid id,
            CancellationToken cancellationToken
        )
        {
            return await _dbSet
                .Where(x =>
                    x.OwnerType == MediaOwnerType.User
                    && x.Category == MediaCategory.Identificate
                    && x.OwnerId == id
                )
                .ToListAsync(cancellationToken);
        }
    }
}
