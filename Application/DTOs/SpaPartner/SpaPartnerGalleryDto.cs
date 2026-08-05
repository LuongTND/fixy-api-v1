namespace Application.DTOs.SpaPartner
{
    public class SpaPartnerGalleryDto
    {
        public Guid Id { get; set; }

        public Guid SpaPartnerId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public string? Caption { get; set; }

        public int SortOrder { get; set; }
    }
}
