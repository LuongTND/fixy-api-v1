using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [AllowAnonymous]
    public class EnumController : ApiController
    {
        private readonly IEnumService _enumService;

        public EnumController(IEnumService enumService)
        {
            _enumService = enumService;
        }

        [HttpGet]
        public async Task<IActionResult> GetEnums(CancellationToken cancellationToken)
        {
            var result = await _enumService.GetEnumsAsync(cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("names")]
        public async Task<IActionResult> GetEnumNames(CancellationToken cancellationToken)
        {
            var result = await _enumService.GetEnumNamesAsync(cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetEnumValues(string name, CancellationToken cancellationToken)
        {
            var result = await _enumService.GetEnumValuesAsync(name, cancellationToken);
            return HandleResult(result);
        }
    }
}
