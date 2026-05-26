using System;
using Domain.Enum;

namespace Application.DTOs.Voucher
{
    public class EligibleVoucherDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public VoucherType Type { get; set; }
        public long Value { get; set; }
        public long MinOrderValue { get; set; }
        public long? MaxDiscount { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsEligible { get; set; }
        public string IneligibleReason { get; set; } = string.Empty;
        public long CalculatedDiscount { get; set; }
    }
}
