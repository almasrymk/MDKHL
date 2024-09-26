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
            CreateMap<City, CityDto>();
            CreateMap<CreateUpdateCityDto, City>();
            CreateMap<Governorate, GovernorateLookupDto>()
               .ForMember(d => d.ArabicName, o => o.MapFrom(s => s.ArabicName))
               .ForMember(d => d.EnglishName, o => o.MapFrom(s => s.EnglishName))
               .ForMember(d => d.ArabicCapital, o => o.MapFrom(s => s.ArabicCapital))
               .ForMember(d => d.EnglishCapital, o => o.MapFrom(s => s.EnglishCapital));

            CreateMap<City, CityLookupDto>()
               .ForMember(d => d.ArabicName, o => o.MapFrom(s => s.ArabicName))
               .ForMember(d => d.EnglishName, o => o.MapFrom(s => s.EnglishName));
        }
    }
}
