using AutoMapper;
using SmartParking.Entities.Lookups;
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
	}
}
