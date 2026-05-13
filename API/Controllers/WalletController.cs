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

        [HttpPost("topup")]
        public async Task<IActionResult> TopUp(
            [FromBody] TopUpRequestDto request,
            CancellationToken cancellationToken
        )
        {
            var result = await _walletService.TopUpAsync(
                request.UserId,
                request.Amount,
                request.ExternalTransactionId,
                cancellationToken
            );

            return HandleResult(result);
        }

        [HttpPost("pay")]
        public async Task<IActionResult> Pay(
            [FromBody] PayRequestDto request,
            CancellationToken cancellationToken
        )
        {
            var result = await _walletService.PayAsync(
                GetUserId(),
                request.Amount,
                request.ReferenceId,
                cancellationToken
            );

            return HandleResult(result);
        }

        [HttpPost("refund")]
        public async Task<IActionResult> Refund(
            [FromBody] RefundRequestDto request,
            CancellationToken cancellationToken
        )
        {
            var result = await _walletService.RefundAsync(
                GetUserId(),
                request.Amount,
                request.ReferenceId,
                cancellationToken
            );

            return HandleResult(result);
        }
    }
}
