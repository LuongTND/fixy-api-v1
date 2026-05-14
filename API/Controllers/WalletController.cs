using System.Security.Claims;
using Application.DTOs.Wallet;
using Application.Interfaces.Services;
using Domain.Enum;
using Microsoft.AspNetCore.Http;
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
            var result = await _walletService.GetWalletAsync(
                GetUserId(),
                WalletOwnerType.Customer,
                cancellationToken
            );

            return HandleResult(result);
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions(CancellationToken cancellationToken)
        {
            var result = await _walletService.GetWalletTransactionAsync(
                GetUserId(),
                WalletOwnerType.Customer,
                cancellationToken
            );

            return HandleResult(result);
        }
    }
}
