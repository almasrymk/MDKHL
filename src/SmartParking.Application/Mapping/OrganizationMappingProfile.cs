using AutoMapper;
using SmartParking.Organizations;


namespace SmartParking.Mapping
{
    public class OrganizationMappingProfile : Profile
    {
        public OrganizationMappingProfile()
        {
            CreateMap<OrganizationDto, CreateUpdateOrganiztionDto>()
             .ForMember(dest => dest.EnglishName, opt => opt.MapFrom(src => src.EnglishName))
             .ForMember(dest => dest.ArabicName, opt => opt.MapFrom(src => src.ArabicName))
             .ForMember(dest => dest.NumOfCameras, opt => opt.MapFrom(src => src.NumOfCameras))
             .ForMember(dest => dest.CommercialRegistrationNumber, opt => opt.MapFrom(src => src.CommercialRegistrationNumber))
             .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
             .ForMember(dest => dest.MaxCapacity, opt => opt.MapFrom(src => src.MaxCapacity))
             .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.ContactMail))
             .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNum))
             .ForMember(dest => dest.WhatsAppNum, opt => opt.MapFrom(src => src.WhatsAppNum))
             .ForMember(dest => dest.FirstHourPrice, opt => opt.MapFrom(src => src.FirstHourPrice))
             .ForMember(dest => dest.SecondHourPrice, opt => opt.MapFrom(src => src.SecondHourPrice))
             .ForMember(dest => dest.ThirdHourPrice, opt => opt.MapFrom(src => src.ThirdHourPrice))
             .ForMember(dest => dest.FullDayPrice, opt => opt.MapFrom(src => src.FullDayPrice))
             .ForMember(dest => dest.NumberOfHours, opt => opt.MapFrom(src => src.NumberOfHours))
             .ForMember(dest => dest.IsNumberOfHoursExceeded, opt => opt.MapFrom(src => src.IsNumberOfHoursExceeded))
             .ForMember(dest => dest.AdditionalHourPrice, opt => opt.MapFrom(src => src.AdditionalHourPrice))
             .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
             .ForMember(dest => dest.BuildingNumber, opt => opt.MapFrom(src => src.BuildingNumber))
             .ForMember(dest => dest.StreetName, opt => opt.MapFrom(src => src.StreetName))
             .ForMember(dest => dest.SecondaryNumber, opt => opt.MapFrom(src => src.SecondaryNumber))
             .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.PostalCode))
             .ForMember(dest => dest.District, opt => opt.MapFrom(src => src.District))
             .ForMember(dest => dest.IsClose, opt => opt.MapFrom(src => src.IsClose))
             .ForMember(dest => dest.OrganizationPicture, opt => opt.MapFrom(src => src.OrganizationPicture))
             .ForMember(dest => dest.SubscriptionId, opt => opt.MapFrom(src => src.SubscriptionId))
             .ForMember(dest => dest.FreeMinutes, opt => opt.MapFrom(src => src.FreeMinutes))
             .ForMember(dest => dest.Graceperiod, opt => opt.MapFrom(src => src.Graceperiod))
             .ForMember(dest => dest.TaxNumber, opt => opt.MapFrom(src => src.TaxNumber))
             .ForMember(dest => dest.HasFreeSlots, opt => opt.MapFrom(src => src.HasFreeSlots))
             .ForMember(dest => dest.HasPackageOnly, opt => opt.MapFrom(src => src.HasPackageOnly))
             .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.TenantId))
             .ForMember(dest => dest.PaymentCode, opt => opt.MapFrom(src => src.PaymentCode))
             .ForMember(dest => dest.CRExpiryDate, opt => opt.MapFrom(src => src.CRExpiryDate));
        }
    }
}
