namespace Application.DTOs.SpaPartner
{
    public class SpaPartnerReviewDto
    {
        public Guid Id { get; set; }

        public Guid SpaPartnerId { get; set; }

        public Guid CustomerProfileId { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string? CustomerAvatar { get; set; }
    }
}
