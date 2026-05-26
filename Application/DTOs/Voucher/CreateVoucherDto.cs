using Domain.Enum;

namespace Application.DTOs.Voucher
{
    public class CreateVoucherDto
    {
        public string Code { get; set; } = string.Empty;
        public VoucherType Type { get; set; }
        public long Value { get; set; }
        public long MinOrderValue { get; set; }
        public long? MaxDiscount { get; set; }
        public Guid? CategoryId { get; set; }
        public int? MaxUsage { get; set; }
        public int? MaxUsagePerUser { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? Description { get; set; }
        public string? City { get; set; }
        public bool? FirstOrderOnly { get; set; }
    }
}
