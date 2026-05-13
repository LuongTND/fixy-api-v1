using Application.Common;
using Application.DTOs.Profile;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;

namespace Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<OperationResult<ProfileDto>> GetProfileAsync(
            Guid userId,
            CancellationToken cancellationToken
        )
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user == null)
            {
                return OperationResult<ProfileDto>.Failure("User not found");
            }

            return OperationResult<ProfileDto>.Success(
                new ProfileDto
                {
                    FullName = user.FullName,
                    Phone = user.Phone,
                    Email = user.Email,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                }
            );
        }

        public async Task<OperationResult<ProfileDto>> UpdateProfileAsync(
            Guid userId,
            UpdateProfileRequestDto dto,
            CancellationToken cancellationToken
        )
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user == null)
            {
                return OperationResult<ProfileDto>.Failure("User not found");
            }
            if (dto.Phone != null)
            {
                var existUser = await _userRepository.GetByTargetAsync(
                    dto.Phone,
                    cancellationToken
                );
                if (existUser != null && existUser.Id != userId)
                {
                    return OperationResult<ProfileDto>.Failure("Phone already exists");
                }
            }
            if (user.Phone != dto.Phone)
            {
                user.IsPhoneVerified = false;
            }

            user.FullName = dto.FullName;
            user.Phone = dto.Phone;
            user.DateOfBirth = dto.DateOfBirth;
            user.Gender = dto.Gender;

            _userRepository.Update(user);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return OperationResult<ProfileDto>.Success(
                new ProfileDto
                {
                    FullName = user.FullName,
                    Phone = user.Phone,
                    Email = user.Email,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                }
            );
        }
    }
}
