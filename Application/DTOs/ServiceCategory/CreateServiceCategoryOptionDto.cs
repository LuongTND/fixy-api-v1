namespace Application.DTOs.ServiceCategory
{
    public class CreateServiceCategoryOptionDto
    {
        public int DurationMinutes { get; set; }

        public long Price { get; set; }

        public int? SortOrder { get; set; }

        public bool? IsActive { get; set; }
    }
}
