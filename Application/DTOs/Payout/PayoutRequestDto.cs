namespace Application.DTOs.Payout
{
    public class PayoutRequestDto
    {
        public Guid Id { get; set; }

        public string PayoutCode { get; set; } = string.Empty;

        public long Amount { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? RejectReason { get; set; }

        public string? GatewayTransactionRef { get; set; }

        public string? VietQrUrl { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? TransferredAt { get; set; }

        public string AccountNumber { get; set; } = string.Empty;

        public string AccountName { get; set; } = string.Empty;

        public string? BankName { get; set; }

        public string? BankCode { get; set; }
    }
}
