using AutoMapper;
using SmartParking.Entities;
using SmartParking.Lookups;
using SmartParking.Lookups.City;

namespace SmartParking.Mapping
{
    public class CityMappingProfile : Profile
    {
        public CityMappingProfile()
        {
            CreateMap<City, CityDto>()
                .ForMember(d=>d.GovernorateEnglishName,o=>o.MapFrom(s=>s.Governorate.EnglishName))
                .ForMember(d=>d.GovernorateArabicName,o=>o.MapFrom(s=>s.Governorate.ArabicName));
            CreateMap<CreateUpdateCityDto, City>();
            CreateMap<Governorate, GovernorateLookupDto>();

            CreateMap<City, CityLookupDto>();
        }
    }
}
