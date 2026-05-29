using System.Security.Claims;
using Application.DTOs.WorkerProfile;
using Application.DTOs.WorkerProfile.WorkerCertificate;
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

        [HttpGet("{workerId:guid}/public")]
        public async Task<IActionResult> GetPublicDetail(
            Guid workerId,
            CancellationToken cancellationToken
        )
        {
            var result = await _workerProfileService.GetPublicDetailAsync(
                workerId,
                cancellationToken
            );

            return HandleResult(result);
        }

        [Authorize]
        [HttpGet("{workerId:guid}/private")]
        public async Task<IActionResult> GetPrivateDetail(
            Guid workerId,
            CancellationToken cancellationToken
        )
        {
            var result = await _workerProfileService.GetPrivateDetailAsync(
                workerId,
                cancellationToken
            );

            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("{workerId:guid}/admin")]
        public async Task<IActionResult> GetAdminDetail(
            Guid workerId,
            CancellationToken cancellationToken
        )
        {
            var result = await _workerProfileService.GetAdminAndOwnerDetailAsync(
                workerId,
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

        [Authorize(Roles = "WORKER")]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile(
            [FromForm] WorkerProfileUpdateRequestDto dto,
            CancellationToken cancellationToken
        )
        {
            var workerId = GetUserId();

            var result = await _workerProfileService.UpdateWorkerProfileAsync(
                workerId,
                dto,
                cancellationToken
            );

            return HandleResult(result);
        }

        [Authorize(Roles = "WORKER")]
        [HttpPost("me/portfolio-images")]
        public async Task<IActionResult> UploadPortfolioImages(
            [FromForm] UploadPortfolioImagesRequestDto dto,
            CancellationToken cancellationToken
        )
        {
            var workerId = GetUserId();

            var result = await _workerProfileService.UploadPortfolioImagesAsync(
                workerId,
                dto,
                cancellationToken
            );

            return HandleResult(result);
        }

        [Authorize(Roles = "WORKER")]
        [HttpDelete("me/portfolio-images/{mediaId:guid}")]
        public async Task<IActionResult> DeletePortfolioImage(
            Guid mediaId,
            CancellationToken cancellationToken
        )
        {
            var workerId = GetUserId();

            var result = await _workerProfileService.DeletePortfolioImageAsync(
                workerId,
                mediaId,
                cancellationToken
            );

            return HandleResult(result);
        }

        [Authorize(Roles = "WORKER")]
        [HttpPut("me/identification-images")]
        public async Task<IActionResult> UpdateIdentificationImages(
            [FromForm] UpdateIdentificationRequestDto dto,
            CancellationToken cancellationToken
        )
        {
            var workerId = GetUserId();

            var result = await _workerProfileService.UpdateIdentificationAsync(
                workerId,
                dto,
                cancellationToken
            );

            return HandleResult(result);
        }

        [Authorize(Roles = "WORKER")]
        [HttpPut("me/centificates")]
        public async Task<IActionResult> UpdateIdentificationImages(
            [FromForm] List<WorkerCertificateUploadRequestDto> dtos,
            CancellationToken cancellationToken
        )
        {
            var workerId = GetUserId();

            var result = await _workerProfileService.UpdateCentificatesAsync(
                workerId,
                dtos,
                cancellationToken
            );

            return HandleResult(result);
        }
    }
}
