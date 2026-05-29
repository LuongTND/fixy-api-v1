using Application.DTOs.VoucherCampaign;
using AutoMapper;
using Domain.Entity;

namespace Application.Mapping
{
    public class CampaignProfile : Profile
    {
        public CampaignProfile()
        {
            CreateMap<VoucherCampaign, CampaignDto>().ReverseMap();
            CreateMap<CreateCampaignDto, VoucherCampaign>().ReverseMap();
            CreateMap<UpdateCampaignDto, VoucherCampaign>().ReverseMap();
        }
    }
}
