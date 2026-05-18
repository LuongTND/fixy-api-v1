using Domain.Common;
using Domain.Enum;

public class WalletTransaction : BaseEntity
{
    public Guid WalletId { get; set; }
    public Wallet? Wallet { get; set; }

    public WalletTransactionType Type { get; set; }

    public WalletDirection Direction { get; set; }

    public long Amount { get; set; }

    public long BalanceBefore { get; set; }

    public long BalanceAfter { get; set; }

    public string? ExternalTransactionId { get; set; }
    public string? ReferenceId { get; set; }

    public TransactionStatus Status { get; set; }
}
