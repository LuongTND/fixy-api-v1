using Application.DTOs.Report;
using Application.Interfaces.Services;
using Application.Validators.Report;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController : ApiController
    {
        private readonly IDashboardService _dashboardService;
        private readonly IReportExportService _reportExportService;

        public DashboardController(
            IDashboardService dashboardService,
            IReportExportService reportExportService
        )
        {
            _dashboardService = dashboardService;
            _reportExportService = reportExportService;
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

        [HttpGet("export")]
        public async Task<IActionResult> ExportReport([FromQuery] ExportReportQuery query)
        {
            // Validate
            var validator = new ExportReportQueryValidator();
            var validationResult = await validator.ValidateAsync(query);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { IsSuccess = false, Errors = errors });
            }

            var result = await _reportExportService.ExportBookingsReportAsync(
                query.Format,
                query.StartDate,
                query.EndDate
            );

            if (!result.IsSuccess)
            {
                return BadRequest(new { IsSuccess = false, Message = result.Message });
            }

            var file = result.Data!;

            return File(file.FileContents, file.ContentType, file.FileName);
        }
    }
}

