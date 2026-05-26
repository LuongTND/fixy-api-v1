using Application.DTOs.Dashboard;

namespace Application.Interfaces.Repositories
{
    public interface IDashboardRepository
    {
        Task<int> GetTotalCustomersAsync();

        Task<int> GetTotalWorkersAsync();

        Task<int> GetTotalBookingsAsync();

        Task<decimal> GetTotalRevenueAsync();

        Task<List<BookingTrendDto>> GetMonthlyBookingTrendAsync(int year);

        Task<List<BookingTrendDto>> GetDailyBookingTrendAsync(int year, int month);

        Task<List<TopServiceDto>> GetTopServicesAsync(int year, int? month = null);
    }
}
