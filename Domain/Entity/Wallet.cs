using Domain.Common;

namespace Domain.Entity
{
    public class Wallet : BaseEntity
    {
        public Guid UserId { get; set; }
        public long Balance { get; set; }
        public long PendingBalance { get; set; }
        public long LifetimeEarned { get; set; }
        public long LifetimeSpent { get; set; }

        public User? User { get; set; }
        public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
    }
}
