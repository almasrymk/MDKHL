using AutoMapper;
using SmartParking.Entities;
using SmartParking.Lookups;

namespace SmartParking.Mapping
{
    public class UserTypeMappingProfile : Profile
    {
        public UserTypeMappingProfile()
        {
            CreateMap<UserType, UserTypeDto>();
            CreateMap<CreateUpdateUserTypeDto, UserType>();
        }
    }
}
