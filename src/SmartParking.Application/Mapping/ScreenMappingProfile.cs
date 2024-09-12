using AutoMapper;
using SmartParking.Entities;
using SmartParking.Lookups;

namespace SmartParking.Mapping
{
    public class ScreenMappingProfile :Profile
    {
        public ScreenMappingProfile()
        {
            CreateMap<Screen , ScreenDto>();
            CreateMap<CreateUpdateScreenDto, Screen>();
        }
    }
}
