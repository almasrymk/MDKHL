using SmartParking.Lookups.City;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups;

public interface ICitiesAppService :ICrudAppService<CityDto,int,PagedAndSortedResultRequestDto,CreateUpdateCityDto>
{
    /// <summary>
    /// Abstract method to create a city.
    /// </summary>
    /// <param name="input">City create DTO.</param>
    /// <returns>CityDto</returns>
    public Task<CityDto> CreateAsync(CreateUpdateCityDto input);

    /// <summary>
    /// Abstract method to update a specific city.
    /// </summary>
    /// <param name="input">City create DTO.</param>
    /// <param name="id">ID of the specific city.</param>
    /// <returns>CityDto</returns>
    public Task<CityDto> UpdateAsync(int id, CreateUpdateCityDto input);

    /// <summary>
    /// Abstract method to delete a specific city.
    /// </summary>
    /// <param name="id">ID of the specific city.</param>
    Task DeleteAsync(int id);

    /// <summary>
    /// Abstract method to list all cities for lookup.
    /// </summary>
    /// <returns>List of CityLookupDto.</returns>
    Task<List<CityLookupDto>> GetCityLookupAsync();

    /// <summary>
    /// Abstract method to search cities.
    /// </summary>
    /// <param name="input">Specifies max number of cities, sorting, and search term.</param>
    /// <returns>Paged result of CityDto.</returns>
    public Task<PagedResultDto<CityDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// Abstract method to filter cities.
    /// </summary>
    /// <param name="input">Specifies max number of cities, sorting, and filter conditions.</param>
    /// <returns>Paged result of CityDto.</returns>
    public Task<PagedResultDto<CityDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);

    /// <summary>
    /// Abstract method to get cities by a specific state.
    /// </summary>
    /// <param name="stateId">ID of the state.</param>
    /// <returns>List of CityLookupDto related to the specified state.</returns>
    public Task<List<CityLookupDto>> GetCityByState(int stateId);

}
