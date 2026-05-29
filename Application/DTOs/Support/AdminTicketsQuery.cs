using Application.Common;
using Domain.Enum;

namespace Application.DTOs.Support
{
    public class AdminTicketsQuery : PagedQuery
    {
        public SupportStatus? Status { get; set; }
        public SupportPriority? Priority { get; set; }
        public SupportReporterType? ReporterType { get; set; }
    }
}
