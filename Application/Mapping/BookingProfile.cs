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
                .ForMember(
                    dest => dest.ScheduledType,
                    opt => opt.MapFrom(src => src.ScheduledType.ToString())
                )
                .ForMember(
                    dest => dest.WorkerProfileId,
                    opt => opt.MapFrom(src => src.WorkerProfileId)
                )
                .ForMember(
                    dest => dest.WorkerName,
                    opt =>
                        opt.MapFrom(src =>
                            src.WorkerProfile != null && src.WorkerProfile.User != null
                                ? src.WorkerProfile.User.FullName
                                : null
                        )
                )
                .ForMember(
                    dest => dest.WorkerPhone,
                    opt =>
                        opt.MapFrom(src =>
                            src.WorkerProfile != null && src.WorkerProfile.User != null
                                ? src.WorkerProfile.User.Phone
                                : null
                        )
                );
        }
    }
}
