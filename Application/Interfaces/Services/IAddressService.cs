using Application.DTOs.Address;

namespace Application.Interfaces.Services
{
    public interface IAddressService
    {
        Task<AddressDto> GetAddressByUserIdAsync(Guid userId);
        Task<AddressDto> CreateAsync(Guid userId, CreateAddressRequestDto dto);
        Task<AddressDto> UpdateAsync(Guid addressId, Guid userId, UpdateAddressRequestDto dto);
        Task DeleteAsync(Guid addressId, Guid userId);
    }
}
