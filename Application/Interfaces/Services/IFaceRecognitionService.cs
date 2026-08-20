using Application.DTOs.WorkerProfile;

namespace Application.Interfaces.Services;

public interface IFaceRecognitionService
{
    Task<FaceMatchResultDto> CompareFacesAsync(
        Stream cardFrontStream,
        Stream selfieStream,
        CancellationToken cancellationToken = default
    );
}
