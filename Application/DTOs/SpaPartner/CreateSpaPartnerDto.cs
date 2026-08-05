using Microsoft.AspNetCore.Http;

namespace Application.DTOs.SpaPartner
{
    public class CreateSpaPartnerDto
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public IFormFile? LogoFile { get; set; }

        public IFormFile? CoverImageFile { get; set; }

        public string Address { get; set; } = string.Empty;

        public string? City { get; set; }

        public double? Lat { get; set; }

        public double? Lng { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? OpeningHours { get; set; }

        public int? SortOrder { get; set; }
    }
}
