using Microsoft.AspNetCore.Http;

namespace Application.DTOs.SpaPartner
{
    public class CreateSpaServiceCategoryDto
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public IFormFile? ImageFile { get; set; }

        public string? Code { get; set; }

        public int? SortOrder { get; set; }

        public bool? IsActive { get; set; }
    }
}
