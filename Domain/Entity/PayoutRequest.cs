using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class PayoutRequest : BaseAuditableEntity
    {
        public Guid WorkerProfileId { get; set; }
        public Guid PayoutAccountId { get; set; }
        public long Amount { get; set; }
        public PayoutRequestStatus Status { get; set; } = PayoutRequestStatus.Pending;
        public Guid? ReviewedById { get; set; }
        public string? RejectReason { get; set; }
        public DateTime? TransferredAt { get; set; }

        public WorkerProfile? Worker { get; set; }
        public WorkerPayoutAccount? PayoutAccount { get; set; }
        public User? ReviewedBy { get; set; }
        public ICollection<WorkerEarning> WorkerEarnings { get; set; } = new List<WorkerEarning>();
        public ICollection<WalletTransaction> WalletTransactions { get; set; } =
            new List<WalletTransaction>();
    }
}
