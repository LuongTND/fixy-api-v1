namespace Application.DTOs.Wallet
{
    public class WalletOverviewDto
    {
        public Guid Id { get; set; }

        public long Balance { get; set; }

        public long LifetimeEarned { get; set; }

        public long LifetimeSpent { get; set; }

        public List<WalletTransactionDto> RecentTransactions { get; set; } = [];
    }
}
