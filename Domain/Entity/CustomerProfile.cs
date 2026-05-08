using Domain.Common;

namespace Domain.Entity
{
    public class CustomerProfile : BaseEntity, ISoftDelete
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string? DeletedBy { get; set; }

        public User? User { get; set; }
        public ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
