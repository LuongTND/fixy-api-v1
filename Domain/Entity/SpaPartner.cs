using Domain.Common;

namespace Domain.Entity
{
    public class SpaPartner : BaseEntity, ISoftDelete
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? LogoUrl { get; set; }

        public string? CoverImageUrl { get; set; }

        public string Address { get; set; } = string.Empty;

        public double? Lat { get; set; }

        public double? Lng { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? City { get; set; }

        public string? OpeningHours { get; set; }

        public double RatingAvg { get; set; }

        public int TotalReviews { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; }

        public DateTime? DeletedDate { get; set; }

        public string? DeletedBy { get; set; }

        public ICollection<SpaPartnerPromotion> Promotions { get; set; } =
            new List<SpaPartnerPromotion>();

        public ICollection<SpaPartnerService> Services { get; set; } =
            new List<SpaPartnerService>();

        public ICollection<SpaPartnerReview> Reviews { get; set; } =
            new List<SpaPartnerReview>();

        public ICollection<SpaPartnerGallery> Gallery { get; set; } =
            new List<SpaPartnerGallery>();
    }
}

