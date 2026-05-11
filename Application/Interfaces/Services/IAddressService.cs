using Application.Common;
using Application.DTOs.Address;

namespace Application.Interfaces.Services
{
    public interface IAddressService
    {
        Task<OperationResult<IEnumerable<AddressDto>>> GetAddressByUserIdAsync(Guid userId);
        Task<OperationResult<AddressDto>> CreateAsync(Guid userId, CreateAddressRequestDto dto);
        Task<OperationResult<AddressDto>> UpdateAsync(
            Guid addressId,
            Guid userId,
            UpdateAddressRequestDto dto
        );
        Task<OperationResult> DeleteAsync(Guid addressId, Guid userId);
    }
}
