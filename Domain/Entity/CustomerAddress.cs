using Domain.Common;

namespace Domain.Entity
{
    public class CustomerAddress : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public string? Label { get; set; }
        public string FullAddress { get; set; } = string.Empty;
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public bool IsDefault { get; set; }

        public CustomerProfile? Customer { get; set; }
    }
}
