namespace Application.DTOs.Payment
{
    public class SePayWebhookDto
    {
        public long Id { get; set; }
        public string Gateway { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string TransferType { get; set; } = string.Empty;
        public long TransferAmount { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? ReferenceCode { get; set; }
        public string? TransactionDate { get; set; }
    }
}
