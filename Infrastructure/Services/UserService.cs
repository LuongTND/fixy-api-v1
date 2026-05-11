using Application.DTOs.Profile;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ProfileDto> GetProfileAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            return new ProfileDto
            {
                FullName = user.FullName,
                Phone = user.Phone,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
            };
        }

        public async Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto dto)
        {
            var user = await _unitOfWork.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            if (user.Phone != dto.Phone)
            {
                user.IsPhoneVerified = false;
            }

            user.FullName = dto.FullName;
            user.Phone = dto.Phone;
            user.DateOfBirth = dto.DateOfBirth;
            user.Gender = dto.Gender;

            _unitOfWork.Users.Update(user);

            await _unitOfWork.SaveChangesAsync();

            return new ProfileDto
            {
                FullName = user.FullName,
                Phone = user.Phone,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
            };
        }
    }
}
