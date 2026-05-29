using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class VoucherRestriction : BaseEntity
    {
        public Guid VoucherId { get; set; }
        public RestrictionType Type { get; set; }
        public string Value { get; set; } = string.Empty;

        // Navigation Property
        public Voucher? Voucher { get; set; }
    }
}
