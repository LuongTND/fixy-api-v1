namespace Application.DTOs.Voucher
{
    public class ApplyVoucherRequest
    {
        public string Code { get; set; } = string.Empty;
        public Guid BookingId { get; set; }
    }
}
