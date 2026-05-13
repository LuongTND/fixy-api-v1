using System.Security.Claims;
using Application.DTOs.Address;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/addresses")]
    [ApiController]
    [Authorize]
    public class AddressController : ApiController
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyAddress(CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var result = await _addressService.GetAddressByUserIdAsync(userId, cancellationToken);

            return HandleResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateAddressRequestDto dto,
            CancellationToken cancellationToken
        )
        {
            var userId = GetUserId();
            var result = await _addressService.CreateAsync(userId, dto, cancellationToken);

            return HandleResult(result);
        }

        [HttpPut("{addressId}")]
        public async Task<IActionResult> Update(
            Guid addressId,
            [FromBody] UpdateAddressRequestDto dto,
            CancellationToken cancellationToken
        )
        {
            var userId = GetUserId();

            var result = await _addressService.UpdateAsync(
                addressId,
                userId,
                dto,
                cancellationToken
            );

            return HandleResult(result);
        }

        [HttpDelete("{addressId}")]
        public async Task<IActionResult> Delete(Guid addressId, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var result = await _addressService.DeleteAsync(addressId, userId, cancellationToken);

            return HandleResult(result);
        }

        private Guid GetUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException();
            }

            return Guid.Parse(userId);
        }
    }
}
