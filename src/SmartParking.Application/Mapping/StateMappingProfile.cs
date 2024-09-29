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
		CreateMap<State, StateDto>();
		CreateMap<CreateUpdateStateDto, State>();
        CreateMap<Region, RegionLookupDto>()
                .ForMember(d => d.ArabicName, o => o.MapFrom(s => s.ArabicName))
                .ForMember(d => d.EnglishName, o => o.MapFrom(s => s.EnglishName));
        CreateMap<State, StateLookupDto>()
				.ForMember(d => d.ArabicName, o => o.MapFrom(s => s.ArabicName))
				.ForMember(d => d.EnglishName, o => o.MapFrom(s => s.EnglishName));      
    }
}
