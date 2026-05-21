using Application.Interfaces.Repositories;
using Domain.Entity;
using Domain.Enum;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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

        public async Task<Media?> GetAvatarByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken
        )
        {
            return await _dbSet
                .Where(x =>
                    x.OwnerType == MediaOwnerType.User
                    && x.Category == MediaCategory.Avatar
                    && x.OwnerId == userId
                )
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<Media>> GetIdentificateImagesByUserId(
            Guid userId,
            CancellationToken cancellationToken
        )
        {
            return await _dbSet
                .Where(x =>
                    x.OwnerType == MediaOwnerType.User
                    && x.Category == MediaCategory.Identification
                    && x.OwnerId == userId
                )
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Media>> GetPorfolioImagesByUserId(
            Guid userId,
            CancellationToken cancellationToken
        )
        {
            return await _dbSet
                .Where(x =>
                    x.OwnerType == MediaOwnerType.WorkerProfile
                    && x.Category == MediaCategory.Portfolio
                    && x.OwnerId == userId
                )
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Media>> GetReviewImagesByReviewIdsAsync(
            List<Guid> reviewIds,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet
                .Where(x =>
                    reviewIds.Contains(x.OwnerId)
                    && x.OwnerType == MediaOwnerType.Review
                    && (x.Category == MediaCategory.Review || x.Category == MediaCategory.Review)
                )
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Media>> GetBookingCompletionImagesAsync(
            Guid bookingId,
            CancellationToken cancellationToken
        )
        {
            return await _dbSet
                .Where(x =>
                    x.OwnerId == bookingId
                    && x.OwnerType == MediaOwnerType.Booking
                    && x.Category == MediaCategory.Completion
                )
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
