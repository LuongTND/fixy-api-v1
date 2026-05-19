using Application.Common;
using Domain.Enum;

namespace Application.DTOs.Booking
{
    public class WorkerBookingsQuery : PagedQuery
    {
        public BookingStatus? Status { get; set; }
        public bool? IsActive { get; set; }
    }
}
