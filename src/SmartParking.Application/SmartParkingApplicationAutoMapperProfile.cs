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
using SmartParking.Entities.Customers;
using SmartParking.Cutomers;
using System;
using SmartParking.Enums;
using SmartParking.Lookups.CameraBrands;
using SmartParking.Lookups.CameraModel;
using SmartParking.Lookups.CameraMainType;

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
            .ForMember(dest => dest.MaxCapacity, opt => opt.MapFrom(src => src.Subscription.ParkingSlot))
            .ForMember(dest => dest.NumOfCameras, opt => opt.MapFrom(src => src.ActiveCameras))
            .ForMember(dest => dest.RegionName, opt => opt.MapFrom(src => src.Region.EnglishName))
            .ForMember(dest => dest.SubscriptionArabicName, opt => opt.MapFrom(src => src.Subscription.ArabicName))
            .ForMember(dest => dest.SubscriptionEnglishName, opt => opt.MapFrom(src => src.Subscription.EnglishName))
            .ForMember(dest => dest.NumberOfUsers, opt => opt.MapFrom(src => src.Subscription.NumberOfUsers))
            .ForMember(dest => dest.NumOfCameras, opt => opt.MapFrom(src => src.Subscription.NumberOfCameras))
            .ForMember(dest => dest.NumberOfCameras, opt => opt.MapFrom(src => src.Subscription.NumberOfCameras))
            .ForMember(dest => dest.NumberOfCustomers, opt => opt.MapFrom(src => src.Subscription.NumberOfCustomers))
            .ForMember(dest => dest.ParkingSlots, opt => opt.MapFrom(src => src.Subscription.ParkingSlot))
            .ForMember(dest => dest.NumbersOfGarages, opt => opt.MapFrom(src => src.Subscription.NumbersOfGarages))
            .ForMember(dest => dest.FreeMinutes, opt => opt.MapFrom(src => src.FreeMinutes))
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
        // camera brand
        CreateMap<SmartParking.Entities.Lookups.CameraBrand, CameraBrandDto>();
		CreateMap<CreateUpdateCameraBrandDto, SmartParking.Entities.Lookups.CameraBrand>();        
        // camera model
        CreateMap<SmartParking.Entities.Lookups.CameraModel, CameraModelDto>();
		CreateMap<CreateUpdateCameraModelDto, SmartParking.Entities.Lookups.CameraModel>();        
        // camera main type
        CreateMap<SmartParking.Entities.Lookups.CameraMainType, CameraMainTypeDto>();
		CreateMap<CreateUpdateCameraMainTypeDto, SmartParking.Entities.Lookups.CameraMainType>();

		CreateMap<VehicleModel, VehicleModelDto>()
             .ForMember(dto => dto.VehicleBrandEnglishName, opt => opt.MapFrom(entity => entity.VehiclesBrand.EnglishName))
             .ForMember(dto => dto.VehicleBrandArabicName, opt => opt.MapFrom(entity => entity.VehiclesBrand.ArabicName));
		CreateMap<CreateUpdateVehicleModelDto, VehicleModel>();
        CreateMap<CreateUpdateVehicleMobileDto, Vehicle>()
			.ForMember(dto => dto.ModelYear, opt => opt.MapFrom(entity => entity.Year));

		CreateMap<Vehicle, VehicleDto>();
		CreateMap<CreateUpdateVehicleDto, Vehicle>();

        CreateMap<Package, PackageDto>()
        .ForMember(dest => dest.PackageType, opt => opt.MapFrom(src => src.PackageType))
        .ForMember(dest => dest.CustomerCategory, opt => opt.MapFrom(src => src.CustomerCategory.ToString()))
        .ForMember(d=>d.PackageTypeArabicName,o=>o.MapFrom(s=>s.PackageType.ArabicName))
        .ForMember(d=>d.PackageTypeEnglishName,o=>o.MapFrom(s=>s.PackageType.EnglishName));
        //.ForMember(dest => dest.Organization, opt => opt.MapFrom(src => src.Organization)); // Map PackageType

        CreateMap<CreateUpdatePackageDto, Package>();
           


        CreateMap<Invoice, InvoiceDto>()
            .ForMember(d => d.GarageName, o => o.MapFrom(s => s.Garage.NameEn))
            .ForMember(d => d.OrganizationArabicName, o => o.MapFrom(s => s.Organization.ArabicName))
            .ForMember(d => d.InvoiceType, o => o.MapFrom(s => s.InvoiceType.ToString()))
            .ForMember(d => d.CustomerArName, o => o.MapFrom(s => s.CustomerArName))
            .ForMember(d => d.CustomerEnName, o => o.MapFrom(s => s.CustomerEnName)).ReverseMap();
            

        CreateMap<Payment, PaymentDto>()
        .ReverseMap();

        CreateMap<Customer, CustomerDto>()
            .ForPath(dest => dest.FullName, opt => opt.MapFrom(src => src.IdentityUser.Name))
            .ForPath(dest => dest.MobileNumber, opt => opt.MapFrom(src => src.IdentityUser.UserName))
            .ForPath(dest => dest.Email, opt => opt.MapFrom(src => src.IdentityUser.NormalizedEmail))
            .ForPath(dest => dest.Email, opt => opt.MapFrom(src => src.IdentityUser.Email))
            .ForPath(dest => dest.MobileNumber, opt => opt.MapFrom(src => src.IdentityUser.PhoneNumber))
            .ForPath(dest => dest.UserName, opt => opt.MapFrom(src => src.IdentityUser.UserName))
            .ForMember(dest => dest.NationalId, opt => opt.MapFrom(src => src.NationalId))
            .ForMember(dest => dest.identificationTypes, opt => opt.MapFrom(src => src.IdentificationTypes))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.OrganizationArabicName, s => s.MapFrom(o => o.CustomerPackages.Select(c => c.Organization.ArabicName).FirstOrDefault() ?? ""))
            .ForMember(dest => dest.OrganizationEnglishName, s => s.MapFrom(o => o.CustomerPackages.Select(c => c.Organization.EnglishName).FirstOrDefault() ?? ""))
            .ForMember(dest => dest.OrganizationId, s => s.MapFrom(o => o.CustomerPackages.Any() ? o.CustomerPackages.Select(c => c.Organization.Id).FirstOrDefault() : 0))
            .ReverseMap();
        CreateMap<Payment, PaymentResponseDto>()
            .ForMember(d => d.PaymentCategory,s => s.MapFrom(o => o.PaymentCategory.ToString()))
            .ForMember(d => d.PaymentMethod,s => s.MapFrom(o => o.PaymentMethod.ToString()))
            .ForMember(d => d.PaymentStatus,s => s.MapFrom(o => o.PaymentStatus.ToString()))
            .ForMember(d => d.Customers,s => s.MapFrom(o => o.Customers))
            //.ForPath(d => d.Customers,s => s.MapFrom(o => o.Customers.ArabicName))
        .ReverseMap();
        CreateMap<Payment, PaymentGetResponseDto>();
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
            //.ForMember(d => d.Role,s => s.MapFrom(o => o.Roles.FirstOrDefault()));

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

