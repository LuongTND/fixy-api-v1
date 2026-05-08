using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class WorkerPayoutAccount : BaseEntity
    {
        public Guid WorkerId { get; set; }
        public WorkerPayoutMethod Method { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string? BankName { get; set; }
        public string? BankCode { get; set; }
        public bool IsDefault { get; set; }
        public bool IsVerified { get; set; }

        public WorkerProfile? Worker { get; set; }
        public ICollection<PayoutRequest> PayoutRequests { get; set; } = new List<PayoutRequest>();
    }
}
