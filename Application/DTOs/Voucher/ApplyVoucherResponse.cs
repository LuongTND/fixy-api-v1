namespace Application.DTOs.Voucher
{
    public class ApplyVoucherResponse
    {
        public string Code { get; set; } = string.Empty;
        public long DiscountAmount { get; set; }
        public long OriginalPrice { get; set; }
        public long FinalPrice { get; set; }
    }
}
