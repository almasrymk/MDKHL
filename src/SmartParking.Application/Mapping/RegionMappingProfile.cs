using AutoMapper;
using SmartParking.Entities;
using SmartParking.Entities.Lookups;
using SmartParking.Lookups;
using SmartParking.Lookups.Region;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Mapping;

public class RegionMappingProfile : Profile
{
	public RegionMappingProfile()
	{
		CreateMap<Region, RegionDto>();
		CreateMap<CretaeUpdateRegionDto, Region>();
	}
}
