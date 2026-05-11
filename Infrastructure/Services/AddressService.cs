using Application.DTOs.Address;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class AddressService : IAddressService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddressService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AddressDto> GetAddressByUserIdAsync(Guid userId)
        {
            var address = await _unitOfWork
                .Addresses.Where(x => x.CustomerId == userId)
                .OrderByDescending(x => x.IsDefault)
                .FirstOrDefaultAsync();

            if (address == null)
            {
                throw new Exception("Address not found");
            }

            return new AddressDto
            {
                Id = address.Id,
                Label = address.Label,
                City = address.City,
                District = address.District,
                Ward = address.Ward,
                Detail = address.Detail,
                Lat = address.Lat,
                Lng = address.Lng,
                IsDefault = address.IsDefault,
                CreatedDate = address.CreatedDate,
            };
        }

        public async Task<AddressDto> CreateAsync(Guid userId, CreateAddressRequestDto dto)
        {
            if (dto.IsDefault)
            {
                var currentDefaults = await _unitOfWork
                    .Addresses.Where(x => x.CustomerId == userId && x.IsDefault)
                    .ToListAsync();

                foreach (var item in currentDefaults)
                {
                    item.IsDefault = false;
                }
            }

            var hasAddress = await _unitOfWork.Addresses.AnyAsync(x => x.CustomerId == userId);

            var address = new Address
            {
                CustomerId = userId,
                Label = dto.Label,
                City = dto.City,
                District = dto.District,
                Ward = dto.Ward,
                Detail = dto.Detail,
                Lat = dto.Lat,
                Lng = dto.Lng,
                IsDefault = !hasAddress || dto.IsDefault,
            };

            await _unitOfWork.Addresses.AddAsync(address);
            await _unitOfWork.SaveChangesAsync();

            return new AddressDto
            {
                Id = address.Id,
                Label = address.Label,
                City = address.City,
                District = address.District,
                Ward = address.Ward,
                Detail = address.Detail,
                Lat = address.Lat,
                Lng = address.Lng,
                IsDefault = address.IsDefault,
                CreatedDate = address.CreatedDate,
            };
        }

        public async Task<AddressDto> UpdateAsync(
            Guid addressId,
            Guid userId,
            UpdateAddressRequestDto dto
        )
        {
            var address = await _unitOfWork.Addresses.FirstOrDefaultAsync(x => x.Id == addressId);

            if (address == null)
            {
                throw new Exception("Address not found");
            }
            if (address.CustomerId != userId)
            {
                throw new UnauthorizedAccessException("You cannot access this address");
            }
            // đổi default
            if (dto.IsDefault && !address.IsDefault)
            {
                var currentDefaults = await _unitOfWork
                    .Addresses.Where(x => x.CustomerId == address.CustomerId && x.IsDefault)
                    .ToListAsync();

                foreach (var item in currentDefaults)
                {
                    item.IsDefault = false;
                }
            }

            address.Label = dto.Label;
            address.City = dto.City;
            address.District = dto.District;
            address.Ward = dto.Ward;
            address.Detail = dto.Detail;
            address.Lat = dto.Lat;
            address.Lng = dto.Lng;
            address.IsDefault = dto.IsDefault;

            _unitOfWork.Addresses.Update(address);

            await _unitOfWork.SaveChangesAsync();

            return new AddressDto
            {
                Id = address.Id,
                Label = address.Label,
                City = address.City,
                District = address.District,
                Ward = address.Ward,
                Detail = address.Detail,
                Lat = address.Lat,
                Lng = address.Lng,
                IsDefault = address.IsDefault,
                CreatedDate = address.CreatedDate,
            };
        }

        public async Task DeleteAsync(Guid addressId, Guid userId)
        {
            var address = await _unitOfWork.Addresses.FirstOrDefaultAsync(x => x.Id == addressId);

            if (address == null)
            {
                throw new Exception("Address not found");
            }
            if (address.CustomerId != userId)
            {
                throw new UnauthorizedAccessException("You cannot access this address");
            }
            address.IsDeleted = true;
            address.DeletedDate = DateTime.UtcNow;

            _unitOfWork.Addresses.Update(address);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
