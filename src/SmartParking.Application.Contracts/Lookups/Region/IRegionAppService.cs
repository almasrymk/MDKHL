using SmartParking.Lookups.City;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups.Region;

public interface IRegionAppService : ICrudAppService<RegionDto, int, PagedAndSortedResultRequestDto, CretaeUpdateRegionDto>
{

}
