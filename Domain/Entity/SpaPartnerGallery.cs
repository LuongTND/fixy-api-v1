using Domain.Common;

namespace Domain.Entity
{
    public class SpaPartnerGallery : BaseEntity
    {
        public Guid SpaPartnerId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public string? Caption { get; set; }

        public int SortOrder { get; set; }

        public SpaPartner? SpaPartner { get; set; }
    }
}
