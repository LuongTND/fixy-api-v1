using System.Security.Claims;
using Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class ApiController : ControllerBase
    {
        protected IActionResult HandleResult<T>(OperationResult<T> result)
        {
            if (result == null)
                return NotFound();

            if (result.IsSuccess && result.Data != null)
                return Ok(result);

            if (result.IsSuccess && result.Data == null)
                return NotFound();

            if (result.Errors != null && result.Errors.Any())
                return BadRequest(result.Errors);

            return BadRequest(result.Message ?? "An error occurred");
        }

        protected IActionResult HandleResult(OperationResult result)
        {
            if (result == null)
                return NotFound();

            if (result.IsSuccess)
                return Ok(result);

            if (result.Errors != null && result.Errors.Any())
                return BadRequest(result.Errors);

            return BadRequest(result.Message ?? "An error occurred");
        }

        protected Guid GetUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException();

            return Guid.Parse(userId);
        }

        protected string GetUserRoles()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;

            if (string.IsNullOrWhiteSpace(role))
                throw new UnauthorizedAccessException("User role not found.");

            return role;
        }
    }
}
