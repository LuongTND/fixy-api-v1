namespace Application.DTOs.SpaPartner
{
    public class SpaPartnerDetailDto : SpaPartnerDto
    {
        public string? Email { get; set; }

        public List<SpaPartnerServiceDto> AllServices { get; set; } = new();

        public List<SpaPartnerGalleryDto> Gallery { get; set; } = new();

        public List<SpaPartnerReviewDto> RecentReviews { get; set; } = new();
    }
}
