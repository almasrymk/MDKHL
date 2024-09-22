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
            CreateMap<Country, CountryLookupDto>();
        }
    }
}
