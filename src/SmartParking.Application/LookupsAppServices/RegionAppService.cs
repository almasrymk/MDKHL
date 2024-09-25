using AutoMapper.Internal.Mappers;
using SmartParking.Entities;
using SmartParking.Lookups.City;
using SmartParking.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartParking.Entities.Lookups;
using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;
using SmartParking.Lookups.Region;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.LookupsAppServices;

public class RegionAppService : CrudAppService<Region, RegionDto,
									int, PagedAndSortedResultRequestDto,
									CretaeUpdateRegionDto>,
		IRegionAppService
{

	public RegionAppService(IRepository<Region, int> repository )
	: base(repository)
	{
	}

	

}
