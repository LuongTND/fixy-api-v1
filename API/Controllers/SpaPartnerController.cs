using Application.Common;
using Application.DTOs.SpaPartner;
using Application.Interfaces.Services.SpaPartner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/spa-partners")]
    public class SpaPartnerController : ApiController
    {
        private readonly ISpaPartnerService _spaPartnerService;

        public SpaPartnerController(ISpaPartnerService spaPartnerService)
        {
            _spaPartnerService = spaPartnerService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] SearchSpaPartnerQuery query, CancellationToken cancellationToken)
        {
            var result = await _spaPartnerService.SearchAsync(query, cancellationToken);
            return HandleResult(result);
        }

        [AllowAnonymous]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetDetail(Guid id, [FromQuery] double? customerLat, [FromQuery] double? customerLng, CancellationToken cancellationToken)
        {
            var result = await _spaPartnerService.GetDetailAsync(id, customerLat, customerLng, cancellationToken);
            return HandleResult(result);
        }

        [AllowAnonymous]
        [HttpGet("nearby")]
        public async Task<IActionResult> GetNearby([FromQuery] double lat, [FromQuery] double lng, [FromQuery] double radiusKm = 10, [FromQuery] int limit = 10, CancellationToken cancellationToken = default)
        {
            var result = await _spaPartnerService.GetNearbyAsync(lat, lng, radiusKm, limit, cancellationToken);
            return HandleResult(result);
        }

        [AllowAnonymous]
        [HttpGet("{id:guid}/reviews")]
        public async Task<IActionResult> GetReviews(Guid id, [FromQuery] PagedQuery query, CancellationToken cancellationToken)
        {
            var result = await _spaPartnerService.GetReviewsAsync(id, query, cancellationToken);
            return HandleResult(result);
        }

        [Authorize]
        [HttpPost("{id:guid}/reviews")]
        public async Task<IActionResult> CreateReview(Guid id, [FromBody] CreateSpaPartnerReviewDto dto, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var result = await _spaPartnerService.CreateReviewAsync(id, userId, dto, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateSpaPartnerDto dto, CancellationToken cancellationToken)
        {
            var result = await _spaPartnerService.CreateAsync(dto, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateSpaPartnerDto dto, CancellationToken cancellationToken)
        {
            var result = await _spaPartnerService.UpdateAsync(id, dto, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await _spaPartnerService.DeleteAsync(id, cancellationToken);
            return HandleResult(result);
        }
    }
}
