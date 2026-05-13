namespace Application.DTOs.Wallet
{
    public class PayRequestDto
    {
        public long Amount { get; set; }
        public string ReferenceId { get; set; } = default!;
    }
}
