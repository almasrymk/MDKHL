using AutoMapper;
using SmartParking.Entities.Lookups;
using SmartParking.Lookups.Region;

namespace SmartParking.Mapping;

public class RegionMappingProfile : Profile
{
	public RegionMappingProfile()
	{
		CreateMap<Region, RegionDto>()
			.ForMember(d=>d.CountryEnglishName,o=>o.MapFrom(s=>s.Country.EnglishName))
			.ForMember(d=>d.CountryArabicName,o=>o.MapFrom(s=>s.Country.ArabicName));

		CreateMap<CreateUpdateRegionDto, Region>();
	}
}
