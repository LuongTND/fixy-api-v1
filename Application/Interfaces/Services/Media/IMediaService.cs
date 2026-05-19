using Application.Common;
using Application.DTOs.Media;

namespace Application.Interfaces.Services.Media
{
    public interface IMediaService
    {
        Task<OperationResult<List<MediaDto>>> UploadMediaAsync(
            UploadMediaFormDto request,
            CancellationToken cancellationToken = default
        );
    }
}
