using Application.DTOs.Support;
using AutoMapper;
using Domain.Entity;

namespace Application.Mapping
{
    public class SupportProfile : Profile
    {
        public SupportProfile()
        {
            CreateMap<SupportTicket, SupportTicketDto>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.HasValue ? src.Category.Value.ToString() : string.Empty))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
