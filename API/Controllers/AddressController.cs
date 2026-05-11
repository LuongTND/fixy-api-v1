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
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAddressRequestDto dto)
        {
            var userId = GetUserId();

            var result = await _addressService.CreateAsync(userId, dto);

            return Ok(result);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyAddress()
        {
            var userId = GetUserId();
            var result = await _addressService.GetAddressByUserIdAsync(userId);

            return Ok(result);
        }

        [HttpPut("{addressId}")]
        public async Task<IActionResult> Update(
            Guid addressId,
            [FromBody] UpdateAddressRequestDto dto
        )
        {
            var result = await _addressService.UpdateAsync(addressId, GetUserId(), dto);

            return Ok(result);
        }

        [HttpDelete("{addressId}")]
        public async Task<IActionResult> Delete(Guid addressId)
        {
            var userId = GetUserId();
            await _addressService.DeleteAsync(addressId, GetUserId());

            return NoContent();
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
