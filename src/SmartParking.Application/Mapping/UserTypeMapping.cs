using AutoMapper;
using SmartParking.Entities;
using SmartParking.Lookups;

namespace SmartParking.Mapping
{
    public class UserTypeMapping : Profile
    {
        public UserTypeMapping()
        {
            CreateMap<UserType, UserTypeDto>();
            CreateMap<CreateUpdateUserTypeDto, UserType>();
        }
    }
}
