using System.ComponentModel.DataAnnotations;
using Domain.Common;
using Domain.Entity;
using Domain.Enum;

public class Wallet : BaseEntity
{
    public Guid UserId { get; set; }

    public WalletOwnerType OwnerType { get; set; }

    public long Balance { get; set; }

    public long LifetimeEarned { get; set; }

    public long LifetimeSpent { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // 🔥 CONCURRENCY TOKEN
    [Timestamp]
    public byte[] RowVersion { get; set; } = default!;
    public User? User { get; set; }

    public ICollection<WalletTransaction> Transactions { get; set; } =
        new List<WalletTransaction>();
}
