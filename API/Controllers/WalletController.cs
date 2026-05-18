using Application.Common;
using Application.Interfaces.Services;
using Domain.Enum;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/wallet")]
    [ApiController]
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
                WalletOwnerType.Customer,
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
                WalletOwnerType.Customer,
                query,
                cancellationToken
            );

            return HandleResult(result);
        }
    }
}
