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
        Task<OperationResult<PagedResponse<WorkerProfileDto>>> GetPagedWorkerRegisterRequest(
            PagedQuery query,
            CancellationToken cancellationToken
        );
        Task<OperationResult<WorkerProfileDetailDto>> GetWorkerProfileDetailRequest(
            Guid id,
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
