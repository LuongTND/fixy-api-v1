using Domain.Common;

namespace Domain.Entity
{
    public class SpaPartnerService : BaseEntity
    {
        public Guid SpaPartnerId { get; set; }

        public Guid SpaServiceCategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public long Price { get; set; }

        public long? DiscountedPrice { get; set; }

        public int DurationMinutes { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public SpaPartner? SpaPartner { get; set; }

        public SpaServiceCategory? SpaServiceCategory { get; set; }
    }
}
