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
            {
                return NotFound(new OperationResult<T>
                {
                    IsSuccess = false,
                    Message = "Resource not found"
                });
            }

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            if (result.Errors.Any())
            {
                return BadRequest(result);
            }

            return BadRequest(result);
        }

        protected IActionResult HandleResult(OperationResult result)
        {
            if (result == null)
            {
                return NotFound(new OperationResult
                {
                    IsSuccess = false,
                    Message = "Resource not found"
                });
            }

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            if (result.Errors.Any())
            {
                return BadRequest(result);
            }

            return BadRequest(result);
        }

        protected Guid GetUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User id not found.");

            return Guid.Parse(userId);
        }

        protected string GetUserRoles()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value
                       ?? User.FindFirst("role")?.Value;

            if (string.IsNullOrWhiteSpace(role))
                throw new UnauthorizedAccessException("User role not found.");

            return role;
        }
    }
}