using Application.Common;
using Application.DTOs.Profile;

namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<OperationResult<ProfileDto>> GetProfileAsync(Guid userId);
        Task<OperationResult<ProfileDto>> UpdateProfileAsync(
            Guid userId,
            UpdateProfileRequestDto dto
        );
    }
}
