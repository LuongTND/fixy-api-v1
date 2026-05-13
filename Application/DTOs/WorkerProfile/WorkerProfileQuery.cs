using Application.Common;
using Domain.Enum;

namespace Application.DTOs.WorkerProfile
{
    public class WorkerProfileQuery : PagedQuery
    {
        public Guid? CategoryId { get; set; }
        public WorkerStatus? Status { get; set; }
    }
}
