using Domain.Enum;

namespace Application.DTOs.Report
{
    public class ExportReportQuery
    {
        public ExportFormat Format { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public ReportType ReportType { get; set; } = ReportType.Bookings;
    }
}
