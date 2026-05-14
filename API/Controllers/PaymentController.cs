using Application.Interfaces.Services.Payment;
using Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ApiController
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [Authorize(Roles = "CUSTOMER")]
        [HttpPost("topup/vnpay")]
        public async Task<IActionResult> CreateTopUpVnPay(
            [FromBody] long amount,
            CancellationToken cancellationToken
        )
        {
            var userId = GetUserId();

            var result = await _paymentService.CreateTopUpPaymentUrlAsync(
                userId,
                amount,
                PaymentMethod.Vnpay,
                cancellationToken
            );

            return HandleResult(result);
        }

        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VnPayReturn(CancellationToken cancellationToken)
        {
            var response = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
            var result = await _paymentService.HandleVnPayCallbackAsync(
                response,
                cancellationToken
            );

            return HandleResult(result);
        }
    }
}
