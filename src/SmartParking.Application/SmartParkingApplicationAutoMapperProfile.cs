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
using SmartParking.CustomerPackages;
using SmartParking.Entities.Subscriptions;
using SmartParking.Subscriptions;
using SmartParking.SubscriptionTypes;
using SmartParking.Analysis;
using System.Collections.Generic;
using System.Linq;
using SmartParking.Mobile.Vehicles;
using SmartParking.Entities.Payments;
using SmartParking.Payments;
using SmartParking.GarageTransactionEndPoints;

namespace SmartParking;


public class ListToDoubleConverter<T> : ITypeConverter<IEnumerable<T>, double>
{
    public double Convert(IEnumerable<T> source, double destination, ResolutionContext context)
    {
        return source != null ? source.Count() : 0;
    }
}
public class SmartParkingApplicationAutoMapperProfile : Profile
{
    public SmartParkingApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        CreateMap<Organization, OrganizationDto>()
            .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City.EnglishName))
            .ForMember(dest => dest.CityNameAr, opt => opt.MapFrom(src => src.City.ArabicName))
            .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Country.EnglishName))
            .ForMember(dest => dest.CountryNameAr, opt => opt.MapFrom(src => src.Country.ArabicName))
            .ForMember(dest => dest.StateName, opt => opt.MapFrom(src => src.State.EnglishName))
            .ForMember(dest => dest.StateNameAr, opt => opt.MapFrom(src => src.State.ArabicName))
            .ForMember(dest => dest.RegionName, opt => opt.MapFrom(src => src.Region.EnglishName))
            .ForMember(dest => dest.MaxCapacity, opt => opt.MapFrom(src => src.ActiveGarages))
            .ForMember(dest => dest.NumOfCameras, opt => opt.MapFrom(src => src.ActiveCameras))
            .ForMember(dest => dest.RegionName, opt => opt.MapFrom(src => src.Region.EnglishName))
            .ForMember(dest => dest.SubscriptionArabicName, opt => opt.MapFrom(src => src.Subscription.ArabicName))
            .ForMember(dest => dest.SubscriptionEnglishName, opt => opt.MapFrom(src => src.Subscription.EnglishName))
            .ForMember(dest => dest.NumberOfUsers, opt => opt.MapFrom(src => src.Subscription.NumberOfUsers))
            .ForMember(dest => dest.NumOfCameras, opt => opt.MapFrom(src => src.Subscription.NumberOfCameras))
            .ForMember(dest => dest.NumberOfCameras, opt => opt.MapFrom(src => src.Subscription.NumberOfCameras))
            .ForMember(dest => dest.NumbersOfGarages, opt => opt.MapFrom(src => src.Subscription.NumbersOfGarages))
            .ForMember(dest => dest.RegionNameAr, opt => opt.MapFrom(src => src.Region.ArabicName)).ReverseMap();
        CreateMap<CreateUpdateOrganiztionDto, Organization>().ReverseMap();
        CreateMap<CreateUpdateOrganiztionDto, OrganizationDto>().ReverseMap();


        CreateMap<CustomerCategory, CustomerCategoryDto>()
            .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization.EnglishName)).ReverseMap();
        CreateMap<CreateUpdateCutomerCategoryDto, CustomerCategory>().ReverseMap();

        CreateMap<ContactInfo, ContactInfoDto>()
        .ReverseMap();
        CreateMap<ContactInfoCreateUpdateDto, ContactInfo>().ReverseMap();

        CreateMap<Garage, GarageDto>()
       .ForMember(dest => dest.OrganizationNameAr, opt => opt.MapFrom(src => src.Organization.ArabicName))
       .ForMember(dest => dest.OrganizationNameEn, opt => opt.MapFrom(src => src.Organization.EnglishName)).ReverseMap();
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
        CreateMap<CreateUpdateVehicleMobileDto, Vehicle>()
			.ForMember(dto => dto.ModelYear, opt => opt.MapFrom(entity => entity.Year));

		CreateMap<Vehicle, VehicleDto>();
		CreateMap<CreateUpdateVehicleDto, Vehicle>();

        CreateMap<Package, PackageDto>()
        .ForMember(dest => dest.PackageType, opt => opt.MapFrom(src => src.PackageType));
        //.ForMember(dest => dest.Organization, opt => opt.MapFrom(src => src.Organization)); // Map PackageType

        CreateMap<CreateUpdatePackageDto, Package>();


        CreateMap<Invoice, InvoiceDto>()
            .ForMember(d => d.GarageName, o => o.MapFrom(s => s.Garage.NameEn))
            .ForMember(d => d.OrganizationArabicName, o => o.MapFrom(s => s.Organization.ArabicName))
            .ForMember(d => d.InvoiceType, o => o.MapFrom(s => s.InvoiceType.ToString()))
            .ForMember(d => d.CustomerArName, o => o.MapFrom(s => s.Customer.ArabicName))
            .ForMember(d => d.CustomerEnName, o => o.MapFrom(s => s.Customer.IdentityUser.Name))
            
            .ForMember(d => d.InvoiceType, o => o.MapFrom(s => s.InvoiceType.ToString()));
        //.ForMember(d => d.PlateNumber, o => o.MapFrom(s => s.Vehicle.PlateNumber));


        CreateMap<Payment, PaymentDto>()
        .ReverseMap();
        CreateMap<CreateUpdatePaymentDto, Payment>().ReverseMap();

        CreateMap<CreateUpdateInvoiceDto, Invoice>();


        CreateMap<Banner, BannerDto>();
        CreateMap<CreateUpdateBannerDto, Banner>();

        CreateMap<GarageTransaction, GarageTransactionDto>();
        CreateMap<CreateUpdateGarageTransactionDto, GarageTransaction>();        

        // unLinked transaction 
        CreateMap<UnLinkedTransaction, UnLinkedTransactionDto>();
        CreateMap<CreateUpdateGarageTransactionEndPointDto, UnLinkedTransaction>();

        CreateMap<CustomerPackage, CustomerPackageDto>();
        CreateMap<CreateUpdateCustomerPackageDto, CustomerPackage>();

        CreateMap<IdentityUser, UserDto>();

        CreateMap<Subscription, SubscriptionDto>()
            .ForMember(d=>d.SubscriptionTypeEn,o=>o.MapFrom(s=>s.SubscriptionType.TypeEn))
            .ForMember(d=>d.SubscriptionTypeAr,o=>o.MapFrom(s=>s.SubscriptionType.TypeAr))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.StatusSubscription.ToString()));

        CreateMap<CreateUpdateSubscriptionDto, Subscription>()
            .ForMember(d => d.CreationTime, o => o.MapFrom(s => s.CreationTime))
            .ForMember(d => d.CreatorId, o => o.MapFrom(s => s.CreatorId));


        CreateMap<SubscriptionType, SubscriptionTypeDto>()
           .ForMember(d => d.TypeEn, o => o.MapFrom(s => s.TypeEn))
           .ForMember(d => d.TypeAr, o => o.MapFrom(s => s.TypeAr));

		CreateMap<CreateUpdateSubscriptionTypeDto, SubscriptionType>();
        CreateMap<SubscriptionType, SubscriptionTypeLookupDto>()
            .ForMember(d => d.SubscriptionTypeEn, o => o.MapFrom(s => s.TypeEn))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status))
            .ForMember(d => d.SubscriptionStatus, o => o.MapFrom(s => s.SubscriptionStatus))
            .ForMember(d => d.SubscriptionTypeAr, o => o.MapFrom(s => s.TypeAr));


        CreateMap<Organization, string>()
        .ConvertUsing(org => org.EnglishName);

        CreateMap<Organization, double>()
       .ConvertUsing(org => org.Customers.Count);



    }
}

