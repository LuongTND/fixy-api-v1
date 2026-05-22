namespace Application.DTOs.Payout
{
    public class CreateWorkerPayoutAccountDto
    {
        public string AccountNumber { get; set; } = string.Empty;

        public string AccountName { get; set; } = string.Empty;

        public string? BankName { get; set; }

        public string? BankCode { get; set; }
    }
}
