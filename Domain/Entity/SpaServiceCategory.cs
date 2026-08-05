using Domain.Common;

namespace Domain.Entity
{
    public class SpaServiceCategory : BaseEntity, ISoftDelete
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public string Code { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; }

        public DateTime? DeletedDate { get; set; }

        public string? DeletedBy { get; set; }

        public ICollection<SpaPartnerService> SpaPartnerServices { get; set; } =
            new List<SpaPartnerService>();
    }
}
