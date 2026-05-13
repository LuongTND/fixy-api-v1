namespace Application.DTOs.ServiceCategory
{
    public class CreateServiceCategoryDto
    {
        public Guid? ParentId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public int? SortOrder { get; set; }

        public bool? IsActive { get; set; }
    }
}
