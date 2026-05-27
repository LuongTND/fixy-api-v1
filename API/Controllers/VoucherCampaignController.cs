using System;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.VoucherCampaign;
using Application.Interfaces.Services.Voucher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize(Roles = "ADMIN")]
    [ApiController]
    [Route("api/admin/voucher-campaigns")]
    public class VoucherCampaignController : ApiController
    {
        private readonly IVoucherCampaignService _campaignService;

        public VoucherCampaignController(IVoucherCampaignService campaignService)
        {
            _campaignService = campaignService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCampaignDto dto, CancellationToken cancellationToken)
        {
            var result = await _campaignService.CreateAsync(dto, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] CampaignQuery query, CancellationToken cancellationToken)
        {
            var result = await _campaignService.GetPagedAsync(query, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _campaignService.GetByIdAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCampaignDto dto, CancellationToken cancellationToken)
        {
            var result = await _campaignService.UpdateAsync(id, dto, cancellationToken);
            return HandleResult(result);
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateCampaignStatusDto dto, CancellationToken cancellationToken)
        {
            var result = await _campaignService.UpdateStatusAsync(id, dto, cancellationToken);
            return HandleResult(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await _campaignService.DeleteAsync(id, cancellationToken);
            return HandleResult(result);
        }
    }
}
