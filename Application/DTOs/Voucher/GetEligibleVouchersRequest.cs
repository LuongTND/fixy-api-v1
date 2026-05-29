using System;

namespace Application.DTOs.Voucher
{
    public class GetEligibleVouchersRequest
    {
        public Guid BookingId { get; set; }
    }
}
