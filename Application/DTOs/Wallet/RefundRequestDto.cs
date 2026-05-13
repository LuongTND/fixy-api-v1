namespace Application.DTOs.Wallet
{
    public class RefundRequestDto
    {
        public long Amount { get; set; }
        public string ReferenceId { get; set; } = default!;
    }
}
