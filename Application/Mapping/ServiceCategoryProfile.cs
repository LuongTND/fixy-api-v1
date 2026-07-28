using Application.DTOs.ServiceCategory;
using AutoMapper;
using Domain.Entity;

namespace Application.Mapping
{
    public class ServiceCategoryProfile : Profile
    {
        public ServiceCategoryProfile()
        {
            CreateMap<CreateServiceCategoryOptionDto, ServiceCategoryOption>()
                .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder ?? 0))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ?? true));

            CreateMap<UpdateServiceCategoryOptionDto, ServiceCategoryOption>();

            CreateMap<ServiceCategoryOption, ServiceCategoryOptionDto>();

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
                )
                .ForMember(dest => dest.Options, opt => opt.Ignore());

            CreateMap<UpdateServiceCategoryDto, ServiceCategory>()
                .ForMember(
                    dest => dest.Name,
                    opt =>
                    {
                        opt.PreCondition(src => src.Name != null);
                        opt.MapFrom(src => src.Name!.Trim());
                    }
                )
                .ForMember(dest => dest.Options, opt => opt.Ignore())
                .ForAllMembers(
                    opt => opt.Condition((src, dest, srcMember) => srcMember != null)
                );
        }
    }
}
