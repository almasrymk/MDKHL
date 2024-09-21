using AutoMapper;
using SmartParking.Entities;
using SmartParking.Lookups;

namespace SmartParking.Mapping
{
    public class PackageTypeMappingProfile : Profile
    {
        public PackageTypeMappingProfile()
        {
            CreateMap<PackageType, PackageTypeDto>();
            CreateMap<CreateUpdatePackageTypeDto, PackageType>();
        }
    }
}
