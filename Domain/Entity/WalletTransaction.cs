using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class WalletTransaction : BaseAuditableEntity
    {
        public Guid WalletId { get; set; }
        public WalletTransactionType Type { get; set; }
        public WalletDirection Direction { get; set; }
        public long Amount { get; set; }
        public long BalanceBefore { get; set; }
        public long BalanceAfter { get; set; }
        public string? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }
        public string? Description { get; set; }
        public TransactionStatus Status { get; set; }
        public Guid? ReversedById { get; set; }

        public Wallet? Wallet { get; set; }
        public WalletTransaction? ReversedBy { get; set; }
    }
}
