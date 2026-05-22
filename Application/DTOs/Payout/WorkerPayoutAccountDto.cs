namespace Application.DTOs.Payout
{
    public class WorkerPayoutAccountDto
    {
        public Guid Id { get; set; }

        public string AccountNumber { get; set; } = string.Empty;

        public string AccountName { get; set; } = string.Empty;

        public string? BankName { get; set; }

        public string? BankCode { get; set; }

        public bool IsDefault { get; set; }

        public bool IsVerified { get; set; }
    }
}
