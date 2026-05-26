using Application.DTOs.Voucher;
using AutoMapper;
using Domain.Entity;
using Domain.Enum;

namespace Application.Mapping
{
    public class VoucherProfile : Profile
    {
        public VoucherProfile()
        {
            CreateMap<Voucher, VoucherDto>()
                .ForMember(dest => dest.MaxUsage,
                    opt => opt.MapFrom(src => src.Quota != null ? src.Quota.MaxUsage : null))
                .ForMember(dest => dest.UsedCount,
                    opt => opt.MapFrom(src => src.Quota != null ? src.Quota.UsedCount : 0))
                .ForMember(dest => dest.MaxUsagePerUser,
                    opt => opt.MapFrom(src => src.Quota != null ? src.Quota.MaxUsagePerUser : null))
                .ForMember(dest => dest.CategoryId,
                    opt => opt.MapFrom(src => GetCategoryId(src)))
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => GetCategoryName(src)))
                .ForMember(dest => dest.City,
                    opt => opt.MapFrom(src => GetCity(src)))
                .ForMember(dest => dest.FirstOrderOnly,
                    opt => opt.MapFrom(src => GetFirstOrderOnly(src)))
                .ForMember(dest => dest.DisplayStatus,
                    opt => opt.MapFrom(src => GetDisplayStatus(src)));

            CreateMap<CreateVoucherDto, Voucher>();
        }

        private static string? GetCity(Voucher src)
        {
            return src.Restrictions?.FirstOrDefault(r => r.Type == RestrictionType.City)?.Value;
        }

        private static bool GetFirstOrderOnly(Voucher src)
        {
            return src.Restrictions?.Any(r => r.Type == RestrictionType.IsFirstOrder) ?? false;
        }

        private static Guid? GetCategoryId(Voucher src)
        {
            var restriction = src.Restrictions?.FirstOrDefault(r => r.Type == RestrictionType.Category);
            if (restriction != null && Guid.TryParse(restriction.Value, out var categoryId))
            {
                return categoryId;
            }
            return null;
        }

        private static string? GetCategoryName(Voucher src)
        {
            // CategoryName is resolved if we eagerly loaded it, but for compatibility, returning null is safe
            return null;
        }

        private static string GetDisplayStatus(Voucher voucher)
        {
            if (voucher.Status == VoucherStatus.Disabled)
                return "Disabled";

            if (voucher.Status == VoucherStatus.Draft)
                return "Draft";

            // Status == Active: check dynamic conditions
            if (DateTime.UtcNow > voucher.ExpiresAt)
                return "Expired";

            if (voucher.Quota != null && voucher.Quota.MaxUsage.HasValue && voucher.Quota.UsedCount >= voucher.Quota.MaxUsage.Value)
                return "OutOfStock";

            return "Active";
        }
    }
}
