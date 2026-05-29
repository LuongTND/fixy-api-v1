using Application.DTOs.Voucher;
using Application.Interfaces.Services.Voucher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/vouchers")]
    public class VoucherController : ApiController
    {
        private readonly IVoucherService _voucherService;

        public VoucherController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        // =============================================
        // Admin Endpoints
        // =============================================

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVoucherDto dto, CancellationToken cancellationToken)
        {
            var result = await _voucherService.CreateAsync(dto, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] VoucherQuery query, CancellationToken cancellationToken)
        {
            var result = await _voucherService.GetPagedAsync(query, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _voucherService.GetByIdAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVoucherDto dto, CancellationToken cancellationToken)
        {
            var result = await _voucherService.UpdateAsync(id, dto, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateVoucherStatusDto dto, CancellationToken cancellationToken)
        {
            var result = await _voucherService.UpdateStatusAsync(id, dto, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await _voucherService.DeleteAsync(id, cancellationToken);
            return HandleResult(result);
        }

        // =============================================
        // Customer Endpoints
        // =============================================

        [Authorize(Roles = "CUSTOMER")]
        [HttpPost("apply")]
        public async Task<IActionResult> ApplyVoucher([FromBody] ApplyVoucherRequest request, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var result = await _voucherService.ApplyVoucherAsync(request, userId, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "CUSTOMER")]
        [HttpPost("eligible")]
        public async Task<IActionResult> GetEligibleVouchers([FromBody] GetEligibleVouchersRequest request, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var result = await _voucherService.GetEligibleVouchersAsync(request, userId, cancellationToken);
            return HandleResult(result);
        }
    }
}
