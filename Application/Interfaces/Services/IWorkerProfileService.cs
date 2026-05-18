using Application.Common;
using Application.DTOs.WorkerProfile;

namespace Application.Interfaces.Services
{
    public interface IWorkerProfileService
    {
        Task<OperationResult> WorkerRegisterAsync(
            WorkerRegisterRequestDto dto,
            CancellationToken cancellationToken
        );
        Task<OperationResult<PagedResponse<WorkerProfileDto>>> GetPagedWorkerProfiles(
            WorkerProfileQuery query,
            string? role,
            CancellationToken cancellationToken
        );
        Task<OperationResult<WorkerPublicDetailDto>> GetPublicDetailAsync(
            Guid workerId,
            CancellationToken cancellationToken
        );
        Task<OperationResult<WorkerPrivateDetailDto>> GetPrivateDetailAsync(
            Guid workerId,
            CancellationToken cancellationToken
        );
        Task<OperationResult<WorkerAdminAndOwnerDetailDto>> GetAdminAndOwnerDetailAsync(
            Guid workerId,
            CancellationToken cancellationToken
        );
        Task<OperationResult> ApproveWorkerRegisterRequest(
            Guid id,
            Guid userId,
            CancellationToken cancellationToken
        );
        Task<OperationResult> RejectWorkerRegisterRequest(
            Guid id,
            string reason,
            CancellationToken cancellationToken
        );
    }
}
