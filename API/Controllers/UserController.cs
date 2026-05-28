using System.Security.Claims;
using Application.DTOs.Profile;
using Application.DTOs.User;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ApiController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public async Task<IActionResult> GetUsers(
            [FromQuery] UserManagementQuery query,
            CancellationToken cancellationToken
        )
        {
            var result = await _userService.GetUsersAsync(query, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
        {
            var userId = GetUserId();

            var result = await _userService.GetProfileAsync(userId, cancellationToken);

            return HandleResult(result);
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateProfile(
            [FromForm] UpdateProfileRequestDto dto,
            CancellationToken cancellationToken
        )
        {
            var userId = GetUserId();

            var result = await _userService.UpdateProfileAsync(userId, dto, cancellationToken);

            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id}/activate")]
        public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
        {
            var result = await _userService.ActivateUserAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
        {
            var result = await _userService.DeactivateUserAsync(id, cancellationToken);
            return HandleResult(result);
        }
    }
}
