using Application.DTOs.Support;
using AutoMapper;
using Domain.Entity;
using System.Linq;

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

            CreateMap<SupportMessage, SupportMessageDto>()
                .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.Sender != null ? src.Sender.FullName : string.Empty))
                .ForMember(dest => dest.SenderRole, opt => opt.MapFrom(src => (src.Sender != null && src.Sender.UserRoles != null) 
                    ? string.Join(", ", src.Sender.UserRoles.Select(ur => ur.Role != null ? ur.Role.Name : string.Empty))
                    : string.Empty));

            CreateMap<SupportTicket, SupportTicketDetailsDto>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.HasValue ? src.Category.Value.ToString() : string.Empty))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => src.Reporter != null ? src.Reporter.FullName : string.Empty))
                .ForMember(dest => dest.ReporterPhone, opt => opt.MapFrom(src => src.Reporter != null ? src.Reporter.Phone : string.Empty))
                .ForMember(dest => dest.ReporterAvatarUrl, opt => opt.MapFrom(src => src.Reporter != null ? src.Reporter.AvatarUrl : string.Empty))
                .ForMember(dest => dest.ReporterType, opt => opt.MapFrom(src => src.ReporterType.ToString()))
                .ForMember(dest => dest.AssignedToName, opt => opt.MapFrom(src => src.AssignedTo != null ? src.AssignedTo.FullName : null))
                .ForMember(dest => dest.Messages, opt => opt.MapFrom(src => src.Messages))
                .ForMember(dest => dest.Booking, opt => opt.MapFrom(src => src.Booking != null ? new BookingSupportDto
                {
                    Id = src.Booking.Id,
                    CategoryName = src.Booking.Category != null ? src.Booking.Category.Name : string.Empty,
                    Status = src.Booking.Status.ToString(),
                    FinalPrice = src.Booking.FinalPrice,
                    Address = src.Booking.Address
                } : null));
        }
    }
}

