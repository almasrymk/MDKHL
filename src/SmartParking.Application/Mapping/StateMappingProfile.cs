using AutoMapper;
using SmartParking.Entities;
using SmartParking.Entities.Lookups;
using SmartParking.Lookups.Governorate;
using SmartParking.Lookups.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Mapping;

public class StateMappingProfile : Profile
{
	public StateMappingProfile()
	{
		CreateMap<State, StateDto>()
			.ForMember(d=>d.RegionArabicName,o=>o.MapFrom(s=>s.Region.ArabicName))
			.ForMember(d => d.RegionEnglishName, o => o.MapFrom(s => s.Region.EnglishName));

		CreateMap<CreateUpdateStateDto, State>();
        CreateMap<Region, RegionLookupDto>();
        CreateMap<State, StateLookupDto>();      
    }
}
