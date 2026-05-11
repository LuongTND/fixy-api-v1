using Application.DTOs.Profile;

namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<ProfileDto> GetProfileAsync(Guid userId);
        Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto dto);
    }
}
