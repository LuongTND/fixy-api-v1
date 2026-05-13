using System.Security.Claims;
using Application.Common;
using Application.DTOs.WorkerProfile;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/workerprofile")]
    [ApiController]
    public class WorkerProfileController : ApiController
    {
        private readonly IWorkerProfileService _workerProfileService;

        public WorkerProfileController(IWorkerProfileService workerProfileService)
        {
            _workerProfileService = workerProfileService;
        }

        [Authorize(Roles = "WORKER")]
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromForm] WorkerRegisterRequestDto dto,
            CancellationToken cancellationToken
        )
        {
            var result = await _workerProfileService.WorkerRegisterAsync(dto, cancellationToken);

            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("requests")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] PagedQuery query,
            CancellationToken cancellationToken
        )
        {
            var result = await _workerProfileService.GetPagedWorkerRegisterRequest(
                query,
                cancellationToken
            );

            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
        {
            var result = await _workerProfileService.GetWorkerProfileDetailRequest(
                id,
                cancellationToken
            );

            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
        {
            // test tạm
            var adminId = GetUserId();

            var result = await _workerProfileService.ApproveWorkerRegisterRequest(
                id,
                adminId,
                cancellationToken
            );

            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(
            Guid id,
            [FromBody] string reason,
            CancellationToken cancellationToken
        )
        {
            var result = await _workerProfileService.RejectWorkerRegisterRequest(
                id,
                reason,
                cancellationToken
            );

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
