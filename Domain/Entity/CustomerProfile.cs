using Domain.Common;

namespace Domain.Entity
{
    public class CustomerProfile : BaseEntity
    {
        public Guid UserId { get; set; }
        public User? User { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
