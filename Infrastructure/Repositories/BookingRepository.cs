using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(AppDbContext context)
            : base(context) { }
    }
}
