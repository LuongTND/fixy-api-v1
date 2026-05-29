using Application.DTOs.Payout;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Authorize(Roles = "WORKER")]
    [Route("api/payout-accounts")]
    public class WorkerPayoutAccountController : ApiController
    {
        private readonly IWorkerPayoutAccountService _workerPayoutAccountService;

        public WorkerPayoutAccountController(IWorkerPayoutAccountService workerPayoutAccountService)
        {
            _workerPayoutAccountService = workerPayoutAccountService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            CreateWorkerPayoutAccountDto dto,
            CancellationToken cancellationToken
        )
        {
            var workerId = GetUserId();

            var result = await _workerPayoutAccountService.CreateAsync(
                workerId,
                dto,
                cancellationToken
            );

            return HandleResult(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
        {
            var workerId = GetUserId();

            var result = await _workerPayoutAccountService.GetMyAccountsAsync(
                workerId,
                cancellationToken
            );

            return HandleResult(result);
        }

        [HttpPut("{id}/default")]
        public async Task<IActionResult> SetDefault(Guid id, CancellationToken cancellationToken)
        {
            var workerId = GetUserId();

            var result = await _workerPayoutAccountService.SetDefaultAsync(
                workerId,
                id,
                cancellationToken
            );

            return HandleResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var workerId = GetUserId();

            var result = await _workerPayoutAccountService.DeleteAsync(
                workerId,
                id,
                cancellationToken
            );

            return HandleResult(result);
        }
    }
}
