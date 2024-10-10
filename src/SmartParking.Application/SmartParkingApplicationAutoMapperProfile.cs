using AutoMapper;
using SmartParking.Cameras;
using SmartParking.CustomerCategories;
using SmartParking.DefaultPackages;
using SmartParking.Entities.Banners;
using SmartParking.Entities.Cameras;
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
using SmartParking.Mobile.Banners;
using SmartParking.Organizations;
using SmartParking.OrganizationsGarages;
using SmartParking.Packages;
using SmartParking.Vehicles;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Invoices;
using Volo.Abp.Identity;
using SmartParking.Authentication;
using SmartParking.Entities.Subscriptions;
using SmartParking.Subscriptions;
using SmartParking.SubscriptionTypes;
using SmartParking.CustomerPackages;

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

        CreateMap<Package, PackageDto>()
        .ForMember(dest => dest.PackageType, opt => opt.MapFrom(src => src.PackageType));
        //.ForMember(dest => dest.Organization, opt => opt.MapFrom(src => src.Organization)); // Map PackageType

        CreateMap<CreateUpdatePackageDto, Package>();


        CreateMap<Invoice,InvoiceDto>()
            .ForMember(d=>d.GarageName,o=>o.MapFrom(s=>s.Garage.Name))
            .ForMember(d=>d.OrganizationArabicName,o=>o.MapFrom(s=>s.Organization.ArabicName))
            .ForMember(d => d.PlateNumber, o => o.MapFrom(s => s.Vehicle.PlateNumber));

        CreateMap<CreateUpdateInvoiceDto, Invoice>();


        CreateMap<Banner, BannerDto>();
        CreateMap<CreateUpdateBannerDto, Banner>();


        CreateMap<DefaultPackage, DefaultPackageDto>();
        CreateMap<CreateUpdateDefaultPackageDto, DefaultPackage>();
    

        CreateMap<GarageTransaction, GarageTransactionDto>();
        CreateMap<CreateUpdateGarageTransactionDto, GarageTransaction>();

        CreateMap<CustomerPackage, CustomerPackageDto>();
        CreateMap<CreateUpdateCustomerPackageDto, CustomerPackage>();

        CreateMap<IdentityUser, UserDto>();

        CreateMap<Subscription, SubscriptionDto>()
            .ForMember(d=>d.SubscriptionType,o=>o.MapFrom(s=>s.SubscriptionType.Type))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<CreateUpdateSubscriptionDto, Subscription>();


        CreateMap<SubscriptionType, SubscriptionTypeDto>()
           .ForMember(d => d.Type, o => o.MapFrom(s => s.Type));

		CreateMap<CreateUpdateSubscriptionTypeDto, SubscriptionType>();
		CreateMap<SubscriptionType, SubscriptionTypeLookupDto>()
			.ForMember(d => d.SubscriptionType, o => o.MapFrom(s => s.Type));
	}
}

