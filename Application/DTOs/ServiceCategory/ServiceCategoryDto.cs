namespace Application.DTOs.ServiceCategory
{
    public class ServiceCategoryDto
    {
        public Guid Id { get; set; }

        public Guid? ParentId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public string Code { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }

        public List<ServiceCategoryOptionDto> Options { get; set; } = new();

        public long? MinPrice => Options != null && Options.Any() ? Options.Min(x => x.Price) : null;

        public long? MaxPrice => Options != null && Options.Any() ? Options.Max(x => x.Price) : null;

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
