using System;
using Application.Common;
using Domain.Enum;

namespace Application.DTOs.Booking
{
    public class AllBookingsQuery : PagedQuery
    {
        public BookingStatus? Status { get; set; }
        public Guid? CustomerProfileId { get; set; }
        public Guid? WorkerProfileId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
