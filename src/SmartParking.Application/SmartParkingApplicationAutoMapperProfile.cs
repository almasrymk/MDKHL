using AutoMapper;
using SmartParking.Cameras;
using SmartParking.CustomerCategories;
using SmartParking.DefaultPackages;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Vehicles;
using SmartParking.Garages;
using SmartParking.Invoices;
using SmartParking.GarageTransactions;
using SmartParking.Lookups.LicenceType;
using SmartParking.Lookups.PlateColor;
using SmartParking.Lookups.VehicleBrand;
using SmartParking.Lookups.VehicleModel;
using SmartParking.Organizations;
using SmartParking.OrganizationsGarages;
using SmartParking.Packages;
using SmartParking.Vehicles;

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
            .ForMember(dest => dest.StateName, opt => opt.MapFrom(src => src.State.ArabicName))
            .ForMember(dest => dest.RegionName, opt => opt.MapFrom(src => src.Region.ArabicName)).ReverseMap();
        CreateMap<CreateUpdateOrganiztionDto, Organization>().ReverseMap();


        CreateMap<CustomerCategory, CustomerCategoryDto>()
            .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization.EnglishName)).ReverseMap();
        CreateMap<CreateUpdateCutomerCategoryDto, CustomerCategory>().ReverseMap();

        CreateMap<Garage, GarageDto>();
        CreateMap<CreateUpdateGarageDto, Garage>().ReverseMap();

        CreateMap<Camera, CameraDto>();
        CreateMap<CreateUpdateCameraDto, Camera>();

        CreateMap<PlateColor, PlateColorDto>();
		CreateMap<CreateUpdatePlateColorDto, PlateColor>();

		CreateMap<LicenceType, LicenceTypeDto>()
            .ForMember(dto=>dto.PlateColorEnglishName,opt=>opt.MapFrom(entity=>entity.PlateColor.EnglishName))
            .ForMember(dto => dto.PlateColorArabicName, opt => opt.MapFrom(entity => entity.PlateColor.ArabicName));
		CreateMap<CreateUpdateLicenceTypeDto, LicenceType>();

		CreateMap<VehicleBrand, VehicleBrandDto>();
		CreateMap<CreateUpdateVehicleBrandDto, VehicleBrand>();

		CreateMap<VehicleModel, VehicleModelDto>()
             .ForMember(dto => dto.VehicleBrandEnglishName, opt => opt.MapFrom(entity => entity.VehiclesBrand.EnglishName))
            .ForMember(dto => dto.VehicleBrandArabicName, opt => opt.MapFrom(entity => entity.VehiclesBrand.ArabicName));
		CreateMap<CreateUpdateVehicleModelDto, VehicleModel>();

		CreateMap<Vehicle, VehicleDto>();
		CreateMap<CreateUpdateVehicleDto, Vehicle>();
	}
}
