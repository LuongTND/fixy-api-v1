using Application.DTOs.ServiceCategory;
using AutoMapper;
using Domain.Entity;

namespace Application.Mapping
{
    public class ServiceCategoryProfile : Profile
    {
        public ServiceCategoryProfile()
        {
            CreateMap<ServiceCategory, ServiceCategoryDto>();

            CreateMap<CreateServiceCategoryDto, ServiceCategory>()
                .ForMember(
                    dest => dest.Name,
                    opt => opt.MapFrom(src => src.Name.Trim())
                )
                .ForMember(
                    dest => dest.SortOrder,
                    opt => opt.MapFrom(src => src.SortOrder ?? 0)
                )
                .ForMember(
                    dest => dest.IsActive,
                    opt => opt.MapFrom(src => src.IsActive ?? true)
                );

            CreateMap<UpdateServiceCategoryDto, ServiceCategory>()
                .ForMember(
                    dest => dest.Name,
                    opt =>
                    {
                        opt.PreCondition(src => src.Name != null);
                        opt.MapFrom(src => src.Name!.Trim());
                    }
                )
                .ForAllMembers(
                    opt => opt.Condition((src, dest, srcMember) => srcMember != null)
                );
        }
    }
}
