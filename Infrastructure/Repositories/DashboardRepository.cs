using Application.DTOs.Dashboard;
using Application.DTOs.Report;
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

        public async Task<List<BookingReportDto>> GetBookingsForReportAsync(
            DateTime startDate,
            DateTime endDate
        )
        {
            var rawData = await _context
                .Bookings.AsNoTracking()
                .Include(b => b.CustomerProfile!)
                .ThenInclude(cp => cp.User!)
                .Include(b => b.WorkerProfile!)
                .ThenInclude(wp => wp.User!)
                .Include(b => b.Category)
                .Where(b => !b.IsDeleted && b.CreatedDate >= startDate && b.CreatedDate <= endDate)
                .OrderByDescending(b => b.CreatedDate)
                .Select(b => new
                {
                    b.Id,
                    CustomerName =
                        b.CustomerProfile != null && b.CustomerProfile.User != null
                            ? b.CustomerProfile.User.FullName
                            : "",
                    WorkerName =
                        b.WorkerProfile != null && b.WorkerProfile.User != null
                            ? b.WorkerProfile.User.FullName
                            : "Chưa nhận",
                    CategoryName =
                        b.Category != null ? b.Category.Name : "",
                    b.Address,
                    b.Status,
                    b.EstimatedPrice,
                    b.FinalPrice,
                    b.CreatedDate,
                    b.CompletedAt,
                })
                .ToListAsync();

            return rawData
                .Select(b => new BookingReportDto
                {
                    BookingId = b.Id,
                    CustomerName = b.CustomerName,
                    WorkerName = b.WorkerName,
                    CategoryName = b.CategoryName,
                    Address = b.Address,
                    Status = MapStatusToVietnamese(b.Status),
                    EstimatedPrice = b.EstimatedPrice ?? 0,
                    FinalPrice = b.FinalPrice ?? 0,
                    CreatedAt = b.CreatedDate,
                    CompletedAt = b.CompletedAt,
                })
                .ToList();
        }

        private static string MapStatusToVietnamese(BookingStatus status)
        {
            return status switch
            {
                BookingStatus.Pending => "Chờ xử lý",
                BookingStatus.PendingPayment => "Chờ thanh toán",
                BookingStatus.Matching => "Đang tìm thợ",
                BookingStatus.Confirmed => "Đã xác nhận",
                BookingStatus.Traveling => "Thợ đang đến",
                BookingStatus.Arrived => "Thợ đã đến",
                BookingStatus.InProgress => "Đang thực hiện",
                BookingStatus.Completed => "Hoàn thành",
                BookingStatus.Cancelled => "Đã hủy",
                BookingStatus.Disputed => "Tranh chấp",
                _ => status.ToString(),
            };
        }
    }
}

