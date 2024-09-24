using AutoMapper;
using SmartParking.Entities;
using SmartParking.Lookups;
using SmartParking.Lookups.Governorate;

namespace SmartParking.Mapping
{
    public  class GovernorateMappingProfile : Profile
    {
        public GovernorateMappingProfile()
        {
            CreateMap<Governorate , GovernorateDto>();
            CreateMap<CreateUpdateGovernorateDto, Governorate>();
            CreateMap<Country, CountryLookupDto>()
                .ForMember(d=>d.ArabicName,o=>o.MapFrom(s=>s.ArabicName))
                .ForMember(d => d.EnglishName, o => o.MapFrom(s => s.EnglishName))
                .ForMember(d => d.ArabicCapital, o => o.MapFrom(s => s.ArabicCapital))
                .ForMember(d => d.EnglishCapital, o => o.MapFrom(s => s.EnglishCapital));
        }
    }
}
