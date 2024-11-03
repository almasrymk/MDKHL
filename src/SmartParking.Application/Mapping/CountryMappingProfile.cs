using AutoMapper;
using SmartParking.Entities;
using SmartParking.Lookups;
using SmartParking.Lookups.Country;

namespace SmartParking.Mapping
{
    public class CountryMappingProfile : Profile
    {
        public CountryMappingProfile()
        {
            CreateMap<Country , CountryDto>();

            CreateMap<CreateUpdateCountryDto, Country>();

			CreateMap<Country, CountryLookupDto>();

		}
	}
}
