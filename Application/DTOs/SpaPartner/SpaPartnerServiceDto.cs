namespace Application.DTOs.SpaPartner
{
    public class SpaPartnerServiceDto
    {
        public Guid Id { get; set; }

        public Guid SpaPartnerId { get; set; }

        public Guid SpaServiceCategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public long Price { get; set; }

        public long? DiscountedPrice { get; set; }

        public int DurationMinutes { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }
    }
}
