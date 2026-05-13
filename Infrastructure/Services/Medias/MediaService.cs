using Application.Common;
using Application.Common.Interfaces;
using Application.DTOs.Media;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Media;
using Application.Settings;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Domain.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Medias
{
    public class MediaService : IMediaService
    {
        private readonly Cloudinary _cloudinary;
        private readonly IMediaRepository _mediaRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<MediaService> _logger;

        public MediaService(
            IOptions<CloudinarySettings> config,
            IMediaRepository mediaRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<MediaService> logger
        )
        {
            var settings = config?.Value ?? throw new ArgumentNullException(nameof(config));
            var acc = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);

            _cloudinary = new Cloudinary(acc);
            _mediaRepository =
                mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _currentUserService =
                currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OperationResult<List<MediaDto>>> UploadMediaAsync(
            UploadMediaFormDto request,
            CancellationToken cancellationToken = default
        )
        {
            if (
                string.IsNullOrWhiteSpace(_currentUserService.UserId)
                || !Guid.TryParse(_currentUserService.UserId, out var uploadedById)
            )
            {
                _logger.LogWarning("User ID not found in token");
                return OperationResult<List<MediaDto>>.Failure("User ID not found in token");
            }

            var uploadedMediaList = new List<Media>();
            var results = new List<MediaDto>();

            foreach (var file in request.Files)
            {
                if (file.Length > 0)
                {
                    await using var stream = file.OpenReadStream();
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        Transformation = new Transformation().Quality("auto").FetchFormat("auto"),
                    };

                    var uploadResult = await _cloudinary.UploadAsync(
                        uploadParams,
                        cancellationToken
                    );

                    if (uploadResult.Error != null)
                    {
                        _logger.LogError(
                            "Cloudinary upload error: {ErrorMessage}",
                            uploadResult.Error.Message
                        );
                        return OperationResult<List<MediaDto>>.Failure(
                            $"Upload failed: {uploadResult.Error.Message}"
                        );
                    }

                    var media = new Media
                    {
                        FileUrl = uploadResult.SecureUrl.ToString(),
                        UploadedById = uploadedById,
                        Category = request.Category,
                        OwnerType = request.OwnerType,
                        OwnerId = request.OwnerId ?? uploadedById,
                    };

                    await _mediaRepository.AddAsync(media, cancellationToken);
                    uploadedMediaList.Add(media);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var media in uploadedMediaList)
            {
                results.Add(
                    new MediaDto
                    {
                        Id = media.Id,
                        FileUrl = media.FileUrl,
                        Category = media.Category.ToString(),
                    }
                );
            }

            _logger.LogInformation(
                "Successfully uploaded {Count} media files for user {UserId}",
                results.Count,
                uploadedById
            );

            return OperationResult<List<MediaDto>>.Success(results, "Upload successful");
        }
    }
}
