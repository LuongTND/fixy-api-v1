using Application.DTOs.SpaPartner;
using Application.Interfaces.Services.SpaPartner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/spa-service-categories")]
    public class SpaServiceCategoryController : ApiController
    {
        private readonly ISpaServiceCategoryService _spaServiceCategoryService;

        public SpaServiceCategoryController(ISpaServiceCategoryService spaServiceCategoryService)
        {
            _spaServiceCategoryService = spaServiceCategoryService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _spaServiceCategoryService.GetAllAsync(cancellationToken);
            return HandleResult(result);
        }

        [AllowAnonymous]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _spaServiceCategoryService.GetByIdAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateSpaServiceCategoryDto dto, CancellationToken cancellationToken)
        {
            var result = await _spaServiceCategoryService.CreateAsync(dto, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] CreateSpaServiceCategoryDto dto, CancellationToken cancellationToken)
        {
            var result = await _spaServiceCategoryService.UpdateAsync(id, dto, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await _spaServiceCategoryService.DeleteAsync(id, cancellationToken);
            return HandleResult(result);
        }
    }
}
