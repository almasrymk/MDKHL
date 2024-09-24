using AutoMapper;
using SmartParking.Cutomers;
using SmartParking.Entities.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Mapping;

public class CustomerMappingProfile : Profile
{
    public CustomerMappingProfile()
    {
        // Map Customer -> CustomerDto
        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.MobileNumber, opt => opt.MapFrom(src => src.MobileNumber))
            .ForMember(dest => dest.NationalId, opt => opt.MapFrom(src => src.NationalId))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

        // Map RegisterCustomerDto -> Customer (reverse mapping)
        CreateMap<RegisterCustomerDto, Customer>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.MobileNumber, opt => opt.MapFrom(src => src.MobileNumber))
            .ForMember(dest => dest.NationalId, opt => opt.MapFrom(src => src.NationalId));
    }
}

