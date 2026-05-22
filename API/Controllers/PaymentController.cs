using Application.DTOs.Payment;
using Application.Interfaces.Services.Payment;
using Domain.Enum;
using Microsoft.AspNetCore.Authorization;
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
        [HttpPost("topup")]
        public async Task<IActionResult> CreateTopUpPayment(
            [FromBody] CreateTopupPaymentRequestDto request,
            CancellationToken cancellationToken
        )
        {
            var result = await _paymentService.CreateTopUpPaymentUrlAsync(
                GetUserId(),
                request.Amount,
                request.Method,
                cancellationToken
            );

            return HandleResult(result);
        }

        [Authorize(Roles = "CUSTOMER")]
        [HttpPost("booking/{bookingId}")]
        public async Task<IActionResult> CreateBookingPayment(
            Guid bookingId,
            [FromBody] CreateBookingPaymentRequestDto request,
            CancellationToken cancellationToken
        )
        {
            var result = await _paymentService.CreateBookingPaymentUrlAsync(
                bookingId,
                GetUserId(),
                request.Method,
                cancellationToken
            );

            return HandleResult(result);
        }

        [HttpGet("callback/vnpay")]
        public async Task<IActionResult> HandleVnPayCallback(CancellationToken cancellationToken)
        {
            var response = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());

            var result = await _paymentService.HandleCallbackAsync(
                PaymentMethod.Vnpay,
                response,
                cancellationToken
            );

            return HandleResult(result);
        }

        [HttpGet("callback/momo")]
        public async Task<IActionResult> HandleMoMoCallback(CancellationToken cancellationToken)
        {
            var response = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());

            var result = await _paymentService.HandleCallbackAsync(
                PaymentMethod.Momo,
                response,
                cancellationToken
            );

            return HandleResult(result);
        }

        [HttpPost("callback/payos")]
        public async Task<IActionResult> HandlePayOSCallback(
            [FromBody] Dictionary<string, object> body,
            CancellationToken cancellationToken
        )
        {
            var response = new Dictionary<string, string>();

            foreach (var item in body)
            {
                response[item.Key] = item.Value?.ToString() ?? string.Empty;
            }

            var result = await _paymentService.HandleCallbackAsync(
                PaymentMethod.PayOS,
                response,
                cancellationToken
            );

            return HandleResult(result);
        }
    }
}
