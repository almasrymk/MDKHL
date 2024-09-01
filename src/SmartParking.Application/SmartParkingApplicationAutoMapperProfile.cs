using AutoMapper;
using SmartParking.CustomerCategories;
using SmartParking.Entities.Organizations;
using SmartParking.Organizations;

namespace SmartParking;

public class SmartParkingApplicationAutoMapperProfile : Profile
{
    public SmartParkingApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        CreateMap<Organization, OrganizationDto>()
            .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City.ArabicName))
            .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Country.ArabicName))
            .ForMember(dest => dest.StatsName, opt => opt.MapFrom(src => src.Stats.ArabicName));
        CreateMap<CreateUpdateOrganiztionDto, Organization>();


        CreateMap<CustomerCategory, CustomerCategoryDto>()
            .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization.EnglishName));
        CreateMap<CreateUpdateCutomerCategoryDto, CustomerCategory>();



    }
}
