using Domain.Enum;

namespace Application.DTOs.Wallet
{
    public class WalletTransactionDto
    {
        public Guid Id { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Direction { get; set; } = string.Empty;

        public long Amount { get; set; }

        public long BalanceBefore { get; set; }

        public long BalanceAfter { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }
    }
}
