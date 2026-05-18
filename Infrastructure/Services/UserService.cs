using Application.Common;
using Application.DTOs.Profile;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Media;
using Domain.Entity;
using Domain.Enum;

namespace Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBlobService _blobService;

        public UserService(
            IUserRepository userRepository,
            IMediaRepository mediaRepository,
            IUnitOfWork unitOfWork,
            IBlobService blobService
        )
        {
            _userRepository = userRepository;
            _mediaRepository = mediaRepository;
            _unitOfWork = unitOfWork;
            _blobService = blobService;
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
                    AvatarUrl = user.AvatarUrl,
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
            user.Gender = dto.Gender.ToString();

            if (dto.Avatar != null)
            {
                string? oldAvatarUrl = user.AvatarUrl;
                string? newAvatarUrl = null;

                try
                {
                    // upload new image
                    newAvatarUrl = await _blobService.UploadImageAsync(dto.Avatar);

                    // remove old avatar media
                    var oldAvatarMedia = await _mediaRepository.GetAvatarByUserIdAsync(
                        userId,
                        cancellationToken
                    );

                    if (oldAvatarMedia != null)
                    {
                        _mediaRepository.Remove(oldAvatarMedia);
                    }

                    // create new media
                    var media = new Media
                    {
                        OwnerId = userId,
                        UploadedById = userId,
                        OwnerType = MediaOwnerType.User,
                        Category = MediaCategory.Avatar,
                        FileUrl = newAvatarUrl,
                    };

                    await _mediaRepository.AddAsync(media, cancellationToken);

                    // update user avatar
                    user.AvatarUrl = newAvatarUrl;

                    // delete old blob after upload success
                    if (!string.IsNullOrWhiteSpace(oldAvatarUrl))
                    {
                        await _blobService.DeleteImageAsync(oldAvatarUrl);
                    }
                }
                catch
                {
                    if (!string.IsNullOrWhiteSpace(newAvatarUrl))
                    {
                        await _blobService.DeleteImageAsync(newAvatarUrl);
                    }

                    throw;
                }
            }

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
                    AvatarUrl = user.AvatarUrl,
                }
            );
        }
    }
}
