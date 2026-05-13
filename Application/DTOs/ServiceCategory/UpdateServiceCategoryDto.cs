namespace Application.DTOs.ServiceCategory
{
    public class UpdateServiceCategoryDto
    {
        public Guid? ParentId { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public int? SortOrder { get; set; }

        public bool? IsActive { get; set; }
    }
}
