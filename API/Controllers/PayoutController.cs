using Application.Common;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/payouts")]
    public class PayoutController : ApiController
    {
        private readonly IPayoutService _payoutService;

        public PayoutController(IPayoutService payoutService)
        {
            _payoutService = payoutService;
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] PagedQuery query,
            CancellationToken cancellationToken
        )
        {
            var result = await _payoutService.GetAllAsync(query, cancellationToken);

            return Ok(result);
        }

        [Authorize(Roles = "WORKER")]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyRequests(
            [FromQuery] PagedQuery query,
            CancellationToken cancellationToken
        )
        {
            var workerId = GetUserId();

            var result = await _payoutService.GetMyRequestsAsync(
                workerId,
                query,
                cancellationToken
            );

            return Ok(result);
        }

        [Authorize(Roles = "WORKER")]
        [HttpPost]
        public async Task<IActionResult> Create(
            Guid payoutAccountId,
            long amount,
            CancellationToken cancellationToken
        )
        {
            var workerId = GetUserId();

            var result = await _payoutService.CreateRequestAsync(
                workerId,
                payoutAccountId,
                amount,
                cancellationToken
            );

            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
        {
            var reviewerId = GetUserId();

            var result = await _payoutService.ApproveAsync(id, reviewerId, cancellationToken);

            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(
            Guid id,
            string reason,
            CancellationToken cancellationToken
        )
        {
            var reviewerId = GetUserId();

            var result = await _payoutService.RejectAsync(
                id,
                reviewerId,
                reason,
                cancellationToken
            );

            return HandleResult(result);
        }
    }
}
