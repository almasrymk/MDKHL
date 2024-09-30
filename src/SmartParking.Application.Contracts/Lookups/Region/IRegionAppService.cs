using SmartParking.Lookups.City;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups.Region;

public interface IRegionAppService : ICrudAppService<RegionDto, int, PagedAndSortedResultRequestDto, CreateUpdateRegionDto>
{
    /// <summary>
    /// abstract method to search of regions
    /// </summary>
    /// <param name="input">specifies max number of region and if it sorted or not and search term</param>
    /// <returns>RegionDto</returns>
    public Task<PagedResultDto<RegionDto>> Search(PagedAndSortedAndSearchResultRequestDto input);


    /// <summary>
    /// abstract method to filter of regions
    /// </summary>
    /// <param name="input">specifies max number of region and if it sorted or not and filter</param>
    /// <returns>RegionDto</returns>
    public Task<PagedResultDto<RegionDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);
}
