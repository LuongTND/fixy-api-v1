using Application.Common;
using Application.Interfaces.Services;
using Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/wallet")]
    [ApiController]
    [Authorize]
    public class WalletController : ApiController
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyWallet(CancellationToken cancellationToken)
        {
            var result = await _walletService.GetWalletOverviewAsync(
                GetUserId(),
                GetUserRoles() == "CUSTOMER" ? WalletOwnerType.Customer : WalletOwnerType.Worker,
                cancellationToken
            );

            return HandleResult(result);
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] PagedQuery query,
            CancellationToken cancellationToken
        )
        {
            var result = await _walletService.GetWalletTransactionsAsync(
                GetUserId(),
                GetUserRoles() == "CUSTOMER" ? WalletOwnerType.Customer : WalletOwnerType.Worker,
                query,
                cancellationToken
            );

            return HandleResult(result);
        }

        [HttpPost("booking/{bookingId}/wallet")]
        public async Task<IActionResult> PayBookingByWallet(
            Guid bookingId,
            CancellationToken cancellationToken
        )
        {
            var result = await _walletService.PayBookingAsync(
                GetUserId(),
                bookingId,
                cancellationToken
            );

            return HandleResult(result);
        }
    }
}
