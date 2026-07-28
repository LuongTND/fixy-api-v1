using Domain.Common;

namespace Domain.Entity
{
    public class ServiceCategoryOption : BaseEntity
    {
        public Guid ServiceCategoryId { get; set; }
        public int DurationMinutes { get; set; }
        public long Price { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public ServiceCategory? Category { get; set; }
    }
}
