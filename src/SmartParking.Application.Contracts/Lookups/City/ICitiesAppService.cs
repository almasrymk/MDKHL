using SmartParking.Lookups.City;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups;

public interface ICitiesAppService :ICrudAppService<CityDto,int,PagedAndSortedResultRequestDto,CreateUpdateCityDto>
{

    /// <summary>
    /// abstract method to create city
    /// </summary>
    /// <param name="input">city create dto</param>
    /// <returns>CityDto</returns>
    public Task<CityDto> CreateAsync(CreateUpdateCityDto input);

    /// <summary>
    /// abstract method to update specific city
    /// </summary>
    /// <param name="input">city create dto</param>
    /// <param name="id">id of specific city</param>
    /// <returns>CityDto</returns>
    public Task<CityDto> UpdateAsync(int id,CreateUpdateCityDto input);
    /// <summary>
    /// abstract method to list all  Governorates
    /// </summary>
    /// <returns>GovernorateLookupDto</returns>
    Task<List<GovernorateLookupDto>> GetGovernorateLookupAsync();

    /// <summary>
    /// abstract method to list all cities
    /// </summary>
    /// <returns>Garage Dto</returns>
    Task<List<CityLookupDto>> GetCityLookupAsync();

    /// <summary>
    /// abstract method to search of cities
    /// </summary>
    /// <param name="input">specifies max number of city and if it sorted or not and search term</param>
    /// <returns>City Dto</returns>
    public Task<PagedResultDto<CityDto>> Search(PagedAndSortedAndSearchResultRequestDto input);


    /// <summary>
    /// abstract method to filter of cities
    /// </summary>
    /// <param name="input">specifies max number of cities and if it sorted or not and filter</param>
    /// <returns>City Dto</returns>
    public Task<PagedResultDto<CityDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);

    Task DeleteAsync(int id);

    public Task<List<CityLookupDto>> GetCityByRegion(int regionId);
}
