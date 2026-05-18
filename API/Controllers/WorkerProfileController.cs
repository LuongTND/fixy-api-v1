using System.Security.Claims;
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
        public async Task<IActionResult> GetPaged(
            [FromQuery] WorkerProfileQuery query,
            CancellationToken cancellationToken
        )
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var result = await _workerProfileService.GetPagedWorkerProfiles(
                query,
                role,
                cancellationToken
            );

            return HandleResult(result);
        }

        [HttpGet("{id:guid}/public")]
        public async Task<IActionResult> GetPublicDetail(
            Guid id,
            CancellationToken cancellationToken
        )
        {
            var result = await _workerProfileService.GetPublicDetailAsync(id, cancellationToken);

            return HandleResult(result);
        }

        [Authorize]
        [HttpGet("{id:guid}/private")]
        public async Task<IActionResult> GetPrivateDetail(
            Guid id,
            CancellationToken cancellationToken
        )
        {
            var result = await _workerProfileService.GetPrivateDetailAsync(id, cancellationToken);

            return HandleResult(result);
        }

        [Authorize(Roles = "WORKER")]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
        {
            var workerId = GetUserId();

            var result = await _workerProfileService.GetAdminAndOwnerDetailAsync(
                workerId,
                cancellationToken
            );

            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("{id:guid}/admin")]
        public async Task<IActionResult> GetAdminDetail(
            Guid id,
            CancellationToken cancellationToken
        )
        {
            var result = await _workerProfileService.GetAdminAndOwnerDetailAsync(
                id,
                cancellationToken
            );

            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
        {
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
