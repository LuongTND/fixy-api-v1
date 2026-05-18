using Microsoft.AspNetCore.Http;

namespace Application.DTOs.ServiceCategory
{
    public class UpdateServiceCategoryDto
    {
        public Guid? ParentId { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public IFormFile? ImageFile { get; set; }

        public int? SortOrder { get; set; }

        public bool? IsActive { get; set; }
    }
}
