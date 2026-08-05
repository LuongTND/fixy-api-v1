namespace Application.DTOs.SpaPartner
{
    public class SpaPartnerPromotionDto
    {
        public Guid Id { get; set; }

        public Guid SpaPartnerId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal DiscountPercent { get; set; }

        public TimeOnly? OffPeakStartTime { get; set; }

        public TimeOnly? OffPeakEndTime { get; set; }

        public DateTime StartsAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsActive { get; set; }

        public bool IsCurrentlyOffPeak { get; set; }
    }
}
