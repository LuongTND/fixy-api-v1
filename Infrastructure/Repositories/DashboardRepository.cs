using Application.DTOs.Dashboard;
using Application.Interfaces.Repositories;
using Domain.Enum;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _context;

        public DashboardRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalCustomersAsync()
        {
            return await _context
                .UserRoles.Include(x => x.Role)
                .CountAsync(x => x.Role!.Name == "CUSTOMER");
        }

        public async Task<int> GetTotalWorkersAsync()
        {
            return await _context
                .UserRoles.Include(x => x.Role)
                .CountAsync(x => x.Role!.Name == "WORKER");
        }

        public async Task<int> GetTotalBookingsAsync()
        {
            return await _context.Bookings.CountAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            var total = await _context
                .Bookings.Where(x => x.Status == BookingStatus.Completed)
                .SumAsync(x => x.FinalPrice ?? 0);

            return Convert.ToDecimal(total);
        }

        public async Task<List<BookingTrendDto>> GetMonthlyBookingTrendAsync(int year)
        {
            var completedData = await _context
                .Bookings.Where(x =>
                    x.Status == BookingStatus.Completed
                    && x.CompletedAt.HasValue
                    && x.CompletedAt.Value.Year == year
                )
                .GroupBy(x => x.CompletedAt!.Value.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();

            var cancelledData = await _context
                .Bookings.Where(x =>
                    x.Status == BookingStatus.Cancelled
                    && x.CancelledAt.HasValue
                    && x.CancelledAt.Value.Year == year
                )
                .GroupBy(x => x.CancelledAt!.Value.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();

            return Enumerable
                .Range(1, 12)
                .Select(month => new BookingTrendDto
                {
                    Label = $"T{month}",

                    CompletedCount =
                        completedData.FirstOrDefault(x => x.Month == month)?.Count ?? 0,

                    CancelledCount =
                        cancelledData.FirstOrDefault(x => x.Month == month)?.Count ?? 0,
                })
                .ToList();
        }

        public async Task<List<BookingTrendDto>> GetDailyBookingTrendAsync(int year, int month)
        {
            var completedData = await _context
                .Bookings.Where(x =>
                    x.Status == BookingStatus.Completed
                    && x.CompletedAt.HasValue
                    && x.CompletedAt.Value.Year == year
                    && x.CompletedAt.Value.Month == month
                )
                .GroupBy(x => x.CompletedAt!.Value.Day)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .ToListAsync();

            var cancelledData = await _context
                .Bookings.Where(x =>
                    x.Status == BookingStatus.Cancelled
                    && x.CancelledAt.HasValue
                    && x.CancelledAt.Value.Year == year
                    && x.CancelledAt.Value.Month == month
                )
                .GroupBy(x => x.CancelledAt!.Value.Day)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .ToListAsync();

            var totalDays = DateTime.DaysInMonth(year, month);

            return Enumerable
                .Range(1, totalDays)
                .Select(day => new BookingTrendDto
                {
                    Label = day.ToString(),

                    CompletedCount = completedData.FirstOrDefault(x => x.Day == day)?.Count ?? 0,

                    CancelledCount = cancelledData.FirstOrDefault(x => x.Day == day)?.Count ?? 0,
                })
                .ToList();
        }

        public async Task<List<TopServiceDto>> GetTopServicesAsync(int year, int? month = null)
        {
            var query = _context
                .Bookings.Include(x => x.Category)
                .Where(x =>
                    x.Status == BookingStatus.Completed
                    && x.CompletedAt.HasValue
                    && x.CompletedAt.Value.Year == year
                );

            if (month.HasValue)
            {
                query = query.Where(x => x.CompletedAt!.Value.Month == month.Value);
            }

            var totalBookings = await query.CountAsync();

            return await query
                .GroupBy(x => x.Category!.Name)
                .Select(g => new TopServiceDto
                {
                    ServiceName = g.Key,

                    TotalBookings = g.Count(),

                    Percentage =
                        totalBookings == 0
                            ? 0
                            : Math.Round((double)g.Count() / totalBookings * 100, 2),
                })
                .OrderByDescending(x => x.TotalBookings)
                .Take(10)
                .ToListAsync();
        }
    }
}
