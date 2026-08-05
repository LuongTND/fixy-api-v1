using Domain.Common;

namespace Domain.Entity
{
    public class SpaPartnerReview : BaseAuditableEntity
    {
        public Guid SpaPartnerId { get; set; }

        public Guid CustomerProfileId { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public bool IsVisible { get; set; } = true;

        public SpaPartner? SpaPartner { get; set; }

        public CustomerProfile? CustomerProfile { get; set; }
    }
}
