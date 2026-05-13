namespace Application.DTOs.Wallet
{
    public class TopUpRequestDto
    {
        public Guid UserId { get; set; }
        public long Amount { get; set; }
        public string ExternalTransactionId { get; set; } = default!;
    }
}
