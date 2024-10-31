
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
    /// <summary>
    /// abstract method to create state
    /// </summary>
    /// <param name="input">state create dto</param>
    /// <returns>StateDto</returns>
    Task<StateDto> CreateAsync(CreateUpdateStateDto input);

    /// <summary>
    /// abstract method to update state
    /// </summary>
    /// <param name="input">state create dto</param>
    /// <param name="id">id of state</param>
    /// <returns>StateDto</returns>
    Task<StateDto> UpdateAsync(int id , CreateUpdateStateDto input);

	/// <summary>
	/// abstract method to get all regions 
	/// </summary>
	/// <returns>RegionLookupDto</returns>
	Task<List<RegionLookupDto>> GetRegionLookupAsync();

	/// <summary>
	/// abstract method to get all states 
	/// </summary>
	/// <returns>StateLookupDto</returns>
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

    /// <summary>
    /// Abstract method to delete state
    /// </summary>
    /// <param name="id">Id of state</param>
    Task DeleteAsync(int id);

	/// <summary>
	/// abstract method to create state
	/// </summary>
	/// <param name="regionId">id of specific region</param>
	/// <returns>StateLookupDto</returns>
	public Task<List<StateLookupDto>> GetStateLookupByRegionAsync(int regionId);
}
