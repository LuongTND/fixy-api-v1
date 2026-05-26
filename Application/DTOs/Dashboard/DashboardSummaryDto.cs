namespace Application.DTOs.Dashboard
{
    public class DashboardSummaryDto
    {
        public int TotalCustomers { get; set; }

        public int TotalWorkers { get; set; }

        public int TotalBookings { get; set; }

        public decimal TotalRevenue { get; set; }
    }
}
