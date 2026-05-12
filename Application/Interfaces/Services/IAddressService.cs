using Application.Common;
using Application.DTOs.Address;

namespace Application.Interfaces.Services
{
    public interface IAddressService
    {
        Task<OperationResult<IEnumerable<AddressDto>>> GetAddressByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken
        );
        Task<OperationResult<AddressDto>> CreateAsync(
            Guid userId,
            CreateAddressRequestDto dto,
            CancellationToken cancellationToken
        );
        Task<OperationResult<AddressDto>> UpdateAsync(
            Guid addressId,
            Guid userId,
            UpdateAddressRequestDto dto,
            CancellationToken cancellationToken
        );
        Task<OperationResult> DeleteAsync(
            Guid addressId,
            Guid userId,
            CancellationToken cancellationToken
        );
    }
}
