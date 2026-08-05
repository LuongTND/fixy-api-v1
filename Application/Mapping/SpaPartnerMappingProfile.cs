using Application.DTOs.SpaPartner;
using AutoMapper;
using Domain.Entity;

namespace Application.Mapping
{
    public class SpaPartnerMappingProfile : Profile
    {
        public SpaPartnerMappingProfile()
        {
            CreateMap<SpaServiceCategory, SpaServiceCategoryDto>()
                .ForMember(dest => dest.SpaCount, opt => opt.MapFrom(src => src.SpaPartnerServices.Select(s => s.SpaPartnerId).Distinct().Count()));

            CreateMap<CreateSpaServiceCategoryDto, SpaServiceCategory>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => !string.IsNullOrWhiteSpace(src.Code) ? src.Code.Trim().ToLower() : src.Name.Trim().ToLower().Replace(" ", "-")))
                .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder ?? 0))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ?? true));

            CreateMap<SpaPartnerService, SpaPartnerServiceDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.SpaServiceCategory != null ? src.SpaServiceCategory.Name : string.Empty));

            CreateMap<SpaPartnerPromotion, SpaPartnerPromotionDto>()
                .ForMember(dest => dest.IsCurrentlyOffPeak, opt => opt.MapFrom(src => IsOffPeakActive(src)));

            CreateMap<SpaPartnerGallery, SpaPartnerGalleryDto>();

            CreateMap<SpaPartnerReview, SpaPartnerReviewDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.CustomerProfile != null && src.CustomerProfile.User != null ? src.CustomerProfile.User.FullName : "Khách hàng"))
                .ForMember(dest => dest.CustomerAvatar, opt => opt.MapFrom(src => src.CustomerProfile != null && src.CustomerProfile.User != null ? src.CustomerProfile.User.AvatarUrl : null));

            CreateMap<SpaPartner, SpaPartnerDto>()
                .ForMember(dest => dest.ActivePromotions, opt => opt.MapFrom(src => src.Promotions.Where(p => p.IsActive && p.StartsAt <= DateTime.UtcNow && p.ExpiresAt >= DateTime.UtcNow)))
                .ForMember(dest => dest.MatchedServices, opt => opt.MapFrom(src => src.Services.Where(s => s.IsActive)));

            CreateMap<SpaPartner, SpaPartnerDetailDto>()
                .ForMember(dest => dest.ActivePromotions, opt => opt.MapFrom(src => src.Promotions.Where(p => p.IsActive && p.StartsAt <= DateTime.UtcNow && p.ExpiresAt >= DateTime.UtcNow)))
                .ForMember(dest => dest.AllServices, opt => opt.MapFrom(src => src.Services.Where(s => s.IsActive)))
                .ForMember(dest => dest.RecentReviews, opt => opt.MapFrom(src => src.Reviews.Where(r => r.IsVisible).OrderByDescending(r => r.CreatedDate).Take(5)));

            CreateMap<CreateSpaPartnerDto, SpaPartner>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()));

            CreateMap<UpdateSpaPartnerDto, SpaPartner>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }

        private static bool IsOffPeakActive(SpaPartnerPromotion promotion)
        {
            if (!promotion.IsActive || promotion.StartsAt > DateTime.UtcNow || promotion.ExpiresAt < DateTime.UtcNow)
                return false;

            if (!promotion.OffPeakStartTime.HasValue || !promotion.OffPeakEndTime.HasValue)
                return true; // Simple promotion active all day within date range

            var nowTime = TimeOnly.FromDateTime(DateTime.Now);
            var start = promotion.OffPeakStartTime.Value;
            var end = promotion.OffPeakEndTime.Value;

            if (start <= end)
            {
                return nowTime >= start && nowTime <= end;
            }
            else
            {
                // Overnight off-peak hours
                return nowTime >= start || nowTime <= end;
            }
        }
    }
}
