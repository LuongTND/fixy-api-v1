using Application.DTOs.Booking;
using AutoMapper;
using Domain.Entity;

namespace Application.Mapping
{
    public class BookingProfile : Profile
    {
        public BookingProfile()
        {
            CreateMap<Booking, BookingDetailDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ScheduledType, opt => opt.MapFrom(src => src.ScheduledType.ToString()));
        }
    }
}
