using Application.Common;
using Application.DTOs.Profile;
using Application.DTOs.User;

namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<OperationResult<PagedResponse<UserManagementDto>>> GetUsersAsync(
            UserManagementQuery query,
            CancellationToken cancellationToken = default
        );
        Task<OperationResult<ProfileDto>> GetProfileAsync(
            Guid userId,
            CancellationToken cancellationToken
        );
        Task<OperationResult<ProfileDto>> UpdateProfileAsync(
            Guid userId,
            UpdateProfileRequestDto dto,
            CancellationToken cancellationToken
        );
    }
}
