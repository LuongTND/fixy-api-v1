using Application.DTOs.ServiceCategory;
using Application.Interfaces.Services.ServiceCategory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/service-categories")]
    public class ServiceCategoryController : ApiController
    {
        private readonly IServiceCategoryService _serviceCategoryService;

        public ServiceCategoryController(IServiceCategoryService serviceCategoryService)
        {
            _serviceCategoryService = serviceCategoryService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _serviceCategoryService.GetAllAsync(cancellationToken);

            return HandleResult(result);
        }

        [AllowAnonymous]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _serviceCategoryService.GetByIdAsync(id, cancellationToken);

            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateServiceCategoryDto dto,CancellationToken cancellationToken)
        {
            var result = await _serviceCategoryService.CreateAsync(dto, cancellationToken);

            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [AllowAnonymous]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceCategoryDto dto, CancellationToken cancellationToken)
        {
            var result = await _serviceCategoryService.UpdateAsync(id, dto, cancellationToken);

            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await _serviceCategoryService.DeleteAsync(id, cancellationToken);

            return HandleResult(result);
        }
    }
}
