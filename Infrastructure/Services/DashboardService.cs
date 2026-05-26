using Application.Common;
using Application.DTOs.Dashboard;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;

namespace Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardService(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<OperationResult<DashboardSummaryDto>> GetDashboardSummaryAsync()
        {
            var dto = new DashboardSummaryDto
            {
                TotalCustomers = await _dashboardRepository.GetTotalCustomersAsync(),

                TotalWorkers = await _dashboardRepository.GetTotalWorkersAsync(),

                TotalBookings = await _dashboardRepository.GetTotalBookingsAsync(),

                TotalRevenue = await _dashboardRepository.GetTotalRevenueAsync(),
            };

            return OperationResult<DashboardSummaryDto>.Success(dto);
        }

        public async Task<OperationResult<List<BookingTrendDto>>> GetBookingTrendsAsync(
            int year,
            int? month = null
        )
        {
            List<BookingTrendDto> result;

            if (month.HasValue)
            {
                result = await _dashboardRepository.GetDailyBookingTrendAsync(year, month.Value);
            }
            else
            {
                result = await _dashboardRepository.GetMonthlyBookingTrendAsync(year);
            }

            return OperationResult<List<BookingTrendDto>>.Success(result);
        }

        public async Task<OperationResult<List<TopServiceDto>>> GetTopServicesAsync(
            int year,
            int? month = null
        )
        {
            var result = await _dashboardRepository.GetTopServicesAsync(year, month);

            return OperationResult<List<TopServiceDto>>.Success(result);
        }
    }
}
