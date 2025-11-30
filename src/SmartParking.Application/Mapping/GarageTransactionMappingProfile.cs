using AutoMapper;
using SmartParking.Entities;
using SmartParking.Entities.Analysis;
using SmartParking.GarageTransactions;
using SmartParking.Lookups;
using SmartParking.Lookups.City;

namespace SmartParking.Mapping
{
    public class GarageTransactionMappingProfile : Profile
    {
        public GarageTransactionMappingProfile()
        {
            CreateMap<GarageTransactionAnalysis, GarageTransactionDto>()
                .ForMember(d=>d.OrganizationArabicName,o=>o.MapFrom(s=>s.OrganizationNameAr))
                .ForMember(d=>d.OrganizationEnglishName,o=>o.MapFrom(s=>s.OrganizationName))
                .ForMember(d=>d.GarageArabicName,o=>o.MapFrom(s=>s.GarageNameAr))
                .ForMember(d=>d.GarageEnglishName,o=>o.MapFrom(s=>s.GarageName))
                ;

            CreateMap<GarageTransactionDto, GarageTransactionAnalysis>()
                            .ForMember(d => d.OrganizationNameAr, o => o.MapFrom(s => s.OrganizationArabicName))
                            .ForMember(d => d.OrganizationName, o => o.MapFrom(s => s.OrganizationEnglishName))
                            .ForMember(d => d.GarageNameAr, o => o.MapFrom(s => s.GarageArabicName))
                            .ForMember(d => d.GarageName, o => o.MapFrom(s => s.GarageEnglishName))
                            ;
        }
    }
}
