using AutoMapper;
using SmartParking.Entities;
using SmartParking.Lookups;

namespace SmartParking.Mapping
{
    public class CountryMappingProfile : Profile
    {
        public CountryMappingProfile()
        {
            CreateMap<Country , CountryDto>();
            CreateMap<CreateUpdateCountryDto, Country>();
        }
    }
}
