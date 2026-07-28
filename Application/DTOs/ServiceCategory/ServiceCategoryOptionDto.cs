namespace Application.DTOs.ServiceCategory
{
    public class ServiceCategoryOptionDto
    {
        public Guid Id { get; set; }

        public Guid ServiceCategoryId { get; set; }

        public int DurationMinutes { get; set; }

        public long Price { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }
    }
}
