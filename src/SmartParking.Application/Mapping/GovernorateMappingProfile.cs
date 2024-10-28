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
            CreateMap<Governorate , GovernorateDto>()
                .ForMember(d=>d.CountryArabicName,o=>o.MapFrom(s=>s.Country.ArabicName))
                .ForMember(d=>d.CountryEnglishName,o=>o.MapFrom(s=>s.EnglishName));

            CreateMap<CreateUpdateGovernorateDto, Governorate>();
            CreateMap<Country, CountryLookupDto>();
        }
    }
}
