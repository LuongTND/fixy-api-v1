using Domain.Common;

namespace Domain.Entity
{
    public class WorkerCertificate : BaseEntity
    {
        public Guid WorkerProfileId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? IssuedBy { get; set; }
        public DateOnly? IssuedAt { get; set; }
        public WorkerProfile? Worker { get; set; }
    }
}
