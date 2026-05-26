using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController : ApiController
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _dashboardService.GetDashboardSummaryAsync();

            return HandleResult(result);
        }

        [HttpGet("booking-trends")]
        public async Task<IActionResult> GetBookingTrends(
            [FromQuery] int year,
            [FromQuery] int? month
        )
        {
            var result = await _dashboardService.GetBookingTrendsAsync(year, month);

            return HandleResult(result);
        }

        [HttpGet("top-services")]
        public async Task<IActionResult> GetTopServices(
            [FromQuery] int year,
            [FromQuery] int? month
        )
        {
            var result = await _dashboardService.GetTopServicesAsync(year, month);

            return HandleResult(result);
        }
    }
}
