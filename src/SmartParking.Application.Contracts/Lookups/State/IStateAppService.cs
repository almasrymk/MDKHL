
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups.State;

public interface IStateAppService : ICrudAppService<StateDto, int, PagedAndSortedResultRequestDto, CreateUpdateStateDto>
{
    Task<List<RegionLookupDto>> GetRegionLookupAsync();
    Task<List<StateLookupDto>> GetStateLookupAsync();

    /// <summary>
    /// abstract method to search of states
    /// </summary>
    /// <param name="input">specifies max number of state and if it sorted or not and search term</param>
    /// <returns>StateDto</returns>
    public Task<PagedResultDto<StateDto>> Search(PagedAndSortedAndSearchResultRequestDto input);


    /// <summary>
    /// abstract method to filter of states
    /// </summary>
    /// <param name="input">specifies max number of state and if it sorted or not and filter</param>
    /// <returns>StateDto</returns>
    public Task<PagedResultDto<StateDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);

}
