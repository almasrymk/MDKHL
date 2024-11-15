using AutoMapper;
using SmartParking.Cutomers;
using SmartParking.Entities.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.AutoMapper;

namespace SmartParking.Mapping;

public class CustomerMappingProfile : Profile
{
    public CustomerMappingProfile()
    {
        // Map Customer -> CustomerDto
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
			.ForMember(dest => dest.OrganizationArabicName, s => s.MapFrom(o => o.CustomerPackages.Select(c => c.Organization.ArabicName).FirstOrDefault()))
			.ForMember(dest => dest.OrganizationEnglishName, s => s.MapFrom(o => o.CustomerPackages.Select(c => c.Organization.EnglishName).FirstOrDefault()))
            .ReverseMap();
          //  .ForMember(dest => dest.OTP, opt => opt.Ignore())
            ;

        // Map RegisterCustomerDto -> Customer (reverse mapping)
        CreateMap<CreateUpdateCustomerDto, Customer>()
            .ForPath(dest => dest.IdentityUser.Name, opt => opt.MapFrom(src => src.FullName))
            .ForPath(dest => dest.IdentityUser.PhoneNumber, opt => opt.MapFrom(src => src.UserName))
            .ForPath(dest => dest.IdentityUser.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForPath(dest => dest.IdentityUser.NormalizedEmail, opt => opt.MapFrom(src => src.Email))
            .ForPath(dest => dest.IdentityUser.Email, opt => opt.MapFrom(src => src.Email))
            .ForPath(dest => dest.IdentityUser.PhoneNumber, opt => opt.MapFrom(src => src.MobileNumber))
            .ForPath(dest => dest.IdentityUser.IsActive, opt => opt.MapFrom(src => src.isActive))
            .ForPath(dest => dest.NationalId, opt => opt.MapFrom(src => src.NationalId)).ReverseMap();

        // Map RegisterCustomerDto -> CustomerDto (reverse mapping)
        CreateMap<CreateUpdateCustomerDto, CustomerDto>()
            .ForPath(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForPath(dest => dest.UserName, opt => opt.MapFrom(src => src.MobileNumber))
            .ForPath(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForPath(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForPath(dest => dest.ArabicName, opt => opt.MapFrom(src => src.ArabicName))
            .ForPath(dest => dest.MobileNumber, opt => opt.MapFrom(src => src.MobileNumber))
            .ForPath(dest => dest.isActive, opt => opt.MapFrom(src => src.isActive))
            .ForPath(dest => dest.identificationTypes, opt => opt.MapFrom(src => src.IdentificationType))
            .ForPath(dest => dest.NationalId, opt => opt.MapFrom(src => src.NationalId)).ReverseMap();




    }
}

