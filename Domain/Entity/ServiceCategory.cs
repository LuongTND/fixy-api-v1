using Domain.Common;

namespace Domain.Entity
{
    public class ServiceCategory : BaseEntity, ISoftDelete
    {
        public Guid? ParentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string Code { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string? DeletedBy { get; set; }

        public ServiceCategory? Parent { get; set; }
        public ICollection<ServiceCategory> Children { get; set; } = new List<ServiceCategory>();
        public ICollection<WorkerService> WorkerServices { get; set; } = new List<WorkerService>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();
    }
}
