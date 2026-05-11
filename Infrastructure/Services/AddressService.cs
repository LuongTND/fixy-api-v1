using Application.Common;
using Application.DTOs.Address;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entity;

namespace Infrastructure.Services
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddressService(IAddressRepository addressRepository, IUnitOfWork unitOfWork)
        {
            _addressRepository = addressRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<OperationResult<IEnumerable<AddressDto>>> GetAddressByUserIdAsync(
            Guid userId
        )
        {
            var addresses = await _addressRepository.GetByUserIdAsync(userId);

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
            var hasAnyAddress = await _addressRepository.ExistsAsync(x => x.CustomerId == userId);

            if (dto.IsDefault)
            {
                var currentDefault = await _addressRepository.GetDefaultByUserIdAsync(userId);
                currentDefault.IsDefault = false;
                _addressRepository.Update(currentDefault);
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

            await _addressRepository.AddAsync(address);
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
            var address = await _addressRepository.GetByIdAsync(addressId, userId);

            if (address == null)
                return OperationResult<AddressDto>.Failure("Address not found or access denied");

            if (dto.IsDefault && !address.IsDefault)
            {
                var currentDefault = await _addressRepository.GetDefaultByUserIdAsync(userId);

                currentDefault.IsDefault = false;
                _addressRepository.Update(currentDefault);
            }

            address.Label = dto.Label;
            address.City = dto.City;
            address.District = dto.District;
            address.Ward = dto.Ward;
            address.Detail = dto.Detail;
            address.Lat = dto.Lat;
            address.Lng = dto.Lng;
            address.IsDefault = dto.IsDefault;

            _addressRepository.Update(address);
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
            var address = await _addressRepository.GetByIdAsync(addressId, userId);

            if (address == null)
                return OperationResult.Failure("Address not found or access denied");

            address.IsDeleted = true;
            address.DeletedDate = DateTime.UtcNow;

            _addressRepository.Update(address);
            await _unitOfWork.SaveChangesAsync();

            return OperationResult.Success("Delete success");
        }
    }
}
