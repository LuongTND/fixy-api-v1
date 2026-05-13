using System.Security.Claims;
using Application.DTOs.Profile;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/user")]
    [Authorize]
    public class UserController : ApiController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
        {
            var userId = GetUserId();

            var result = await _userService.GetProfileAsync(userId, cancellationToken);

            return HandleResult(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateProfileRequestDto dto,
            CancellationToken cancellationToken
        )
        {
            var userId = GetUserId();

            var result = await _userService.UpdateProfileAsync(userId, dto, cancellationToken);

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
