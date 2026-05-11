using Application.Common;
using Application.DTOs.Address;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Entity;

namespace Infrastructure.Services
{
    public class AddressService : IAddressService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddressService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OperationResult<IEnumerable<AddressDto>>> GetAddressByUserIdAsync(
            Guid userId
        )
        {
            var addresses = await _unitOfWork.Addresses.GetByUserIdAsync(userId);

            var result = addresses
                .OrderByDescending(x => x.IsDefault)
                .Select(address => new AddressDto
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
                });

            return OperationResult<IEnumerable<AddressDto>>.Success(result);
        }

        public async Task<OperationResult<AddressDto>> CreateAsync(
            Guid userId,
            CreateAddressRequestDto dto
        )
        {
            var hasAnyAddress = await _unitOfWork.Addresses.ExistsAsync(x =>
                x.CustomerId == userId
            );

            if (dto.IsDefault)
            {
                var currentDefaults = await _unitOfWork.Addresses.GetDefaultByUserIdAsync(userId);

                foreach (var item in currentDefaults)
                {
                    item.IsDefault = false;
                    _unitOfWork.Addresses.Update(item);
                }
            }

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
                IsDefault = !hasAnyAddress || dto.IsDefault,
            };

            await _unitOfWork.Addresses.AddAsync(address);
            await _unitOfWork.SaveChangesAsync();

            return OperationResult<AddressDto>.Success(
                new AddressDto
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
                }
            );
        }

        public async Task<OperationResult<AddressDto>> UpdateAsync(
            Guid addressId,
            Guid userId,
            UpdateAddressRequestDto dto
        )
        {
            var address = await _unitOfWork.Addresses.GetByIdAndUserAsync(addressId, userId);

            if (address == null)
                return OperationResult<AddressDto>.Failure("Address not found or access denied");

            if (dto.IsDefault && !address.IsDefault)
            {
                var currentDefaults = await _unitOfWork.Addresses.GetDefaultByUserIdAsync(userId);

                foreach (var item in currentDefaults)
                {
                    item.IsDefault = false;
                    _unitOfWork.Addresses.Update(item);
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

            return OperationResult<AddressDto>.Success(
                new AddressDto
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
                }
            );
        }

        public async Task<OperationResult> DeleteAsync(Guid addressId, Guid userId)
        {
            var address = await _unitOfWork.Addresses.GetByIdAndUserAsync(addressId, userId);

            if (address == null)
                return OperationResult.Failure("Address not found or access denied");

            address.IsDeleted = true;
            address.DeletedDate = DateTime.UtcNow;

            _unitOfWork.Addresses.Update(address);
            await _unitOfWork.SaveChangesAsync();

            return OperationResult.Success("Delete success");
        }
    }
}
