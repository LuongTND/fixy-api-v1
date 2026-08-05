namespace Application.DTOs.SpaPartner
{
    public class SpaPartnerDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? LogoUrl { get; set; }

        public string? CoverImageUrl { get; set; }

        public string Address { get; set; } = string.Empty;

        public string? City { get; set; }

        public double? Lat { get; set; }

        public double? Lng { get; set; }

        public string? Phone { get; set; }

        public string? OpeningHours { get; set; }

        public double RatingAvg { get; set; }

        public int TotalReviews { get; set; }

        public bool IsActive { get; set; }

        public double? DistanceKm { get; set; }

        public List<SpaPartnerPromotionDto> ActivePromotions { get; set; } = new();

        public List<SpaPartnerServiceDto> MatchedServices { get; set; } = new();
    }
}
