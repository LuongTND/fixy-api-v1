using Application.Common;
using Domain.Enum;

namespace Application.DTOs.Booking
{
    public class CustomerBookingsQuery : PagedQuery
    {
        public BookingStatus? Status { get; set; }
        public bool? IsActive { get; set; }
    }
}
