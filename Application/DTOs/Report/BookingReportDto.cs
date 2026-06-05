namespace Application.DTOs.Report
{
    public class BookingReportDto
    {
        public Guid BookingId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string WorkerName { get; set; } = "Chưa nhận";

        public string CategoryName { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public long EstimatedPrice { get; set; }

        public long FinalPrice { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}
