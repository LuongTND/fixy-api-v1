using System.Security.Claims;
using Application.Common;
using Application.DTOs.WorkerProfile;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/worker-profiles")]
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

        [HttpGet]
        public async Task<IActionResult> GetAdminProfiles(
            [FromQuery] WorkerProfileQuery query,
            CancellationToken cancellationToken
        )
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var result = await _workerProfileService.GetPagedWorkerProfiles(
                query,
                userRole,
                cancellationToken
            );

            return HandleResult(result);
        }

        [Authorize(Roles = "WORKER")]
        [HttpGet("me")]
        public async Task<IActionResult> GetDetail(CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var result = await _workerProfileService.GetWorkerProfileDetail(
                userId,
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
        [HttpPut("{id:guid}/reject")]
        public async Task<IActionResult> Reject(
            Guid id,
            [FromBody] RejectWorkerProfileRequestDto dto,
            CancellationToken cancellationToken
        )
        {
            var result = await _workerProfileService.RejectWorkerRegisterRequest(
                id,
                dto.Reason,
                cancellationToken
            );

            return HandleResult(result);
        }
    }
}
