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
                .ForMember(d=>d.StateEnglishName,o=>o.MapFrom(s=>s.State.EnglishName))
                .ForMember(d=>d.StateArabicName,o=>o.MapFrom(s=>s.State.ArabicName));

            CreateMap<CreateUpdateCityDto, City>();

            CreateMap<City, CityLookupDto>();
        }
    }
}
