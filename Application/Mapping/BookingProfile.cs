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
                .ForMember(dest => dest.ScheduledType, opt => opt.MapFrom(src => src.ScheduledType.ToString()))
                .ForMember(dest => dest.WorkerProfileId, opt => opt.MapFrom(src => src.WorkerId))
                .ForMember(dest => dest.WorkerName, opt => opt.MapFrom(src => src.Worker != null && src.Worker.User != null ? src.Worker.User.FullName : null))
                .ForMember(dest => dest.WorkerPhone, opt => opt.MapFrom(src => src.Worker != null && src.Worker.User != null ? src.Worker.User.Phone : null));
        }
    }
}
