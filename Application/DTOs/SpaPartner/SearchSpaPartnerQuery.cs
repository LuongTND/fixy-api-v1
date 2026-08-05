using Application.Common;

namespace Application.DTOs.SpaPartner
{
    public class SearchSpaPartnerQuery : PagedQuery
    {
        public Guid? SpaServiceCategoryId { get; set; }

        public string? City { get; set; }

        public double? CustomerLat { get; set; }

        public double? CustomerLng { get; set; }

        public double? MaxDistanceKm { get; set; }

        public double? MinRating { get; set; }

        public bool? HasPromotion { get; set; }

        public bool? IsOffPeakNow { get; set; }
    }
}
