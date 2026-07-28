namespace Application.DTOs.ServiceCategory
{
    public class UpdateServiceCategoryOptionDto
    {
        public Guid? Id { get; set; }

        public int DurationMinutes { get; set; }

        public long Price { get; set; }

        public int? SortOrder { get; set; }

        public bool? IsActive { get; set; }
    }
}
