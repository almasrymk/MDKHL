using AutoMapper;
using SmartParking.Entities;
using SmartParking.Lookups;

namespace SmartParking.Mapping
{
    public  class GovernorateMappingProfile : Profile
    {
        public GovernorateMappingProfile()
        {
            CreateMap<Governorate , GovernorateDto>();
            CreateMap<CreateUpdateGovernorateDto, Governorate>();
        }
    }
}
