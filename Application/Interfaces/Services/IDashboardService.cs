using Application.Common;
using Application.DTOs.Dashboard;

namespace Application.Interfaces.Services;

public interface IDashboardService
{
    Task<OperationResult<DashboardSummaryDto>> GetDashboardSummaryAsync();

    Task<OperationResult<List<BookingTrendDto>>> GetBookingTrendsAsync(int year, int? month = null);

    Task<OperationResult<List<TopServiceDto>>> GetTopServicesAsync(int year, int? month = null);
}
