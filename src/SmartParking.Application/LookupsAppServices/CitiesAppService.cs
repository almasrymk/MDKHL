using Microsoft.AspNetCore.Authorization;
using SmartParking.Entities;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Lookups;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Filters;
using SmartParking.Lookups;
using SmartParking.Lookups.City;
using SmartParking.Lookups.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.LookupsAppServices;
[Authorize]


public class CitiesAppService : CrudAppService<City, CityDto,
                                int, PagedAndSortedResultRequestDto,
                                CreateUpdateCityDto>,
    ICitiesAppService

{
    private readonly IRepository<City, int> _repository;

    public CitiesAppService(IRepository<City, int> repository)
    : base(repository)
    {
        _repository = repository;
    }

    ///<inheritdoc/>
   public override async Task<CityDto> CreateAsync(CreateUpdateCityDto input)
    {
        await CheckCityUniquness(input);

        return await base.CreateAsync(input);

    }

    private async Task CheckCityUniquness(CreateUpdateCityDto input)
    {
        var city = await _repository.FirstOrDefaultAsync
                 (city =>
                 city.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
                 city.ArabicName.ToLower().Contains(input.ArabicName.ToLower()));

        if (city != null)
        {
            if (city.EnglishName.ToLower().Trim(' ') == input.EnglishName.ToLower().Trim(' '))
            {
                throw new InvalidOperationException("city english name already exists");

            }
            else if (city.ArabicName.ToLower().Trim(' ') == input.ArabicName.ToLower().Trim(' '))
            {
                throw new InvalidOperationException("city arabic name already exists");

            }

        }
    }

    ///<inheritdoc/>
    public override async Task<CityDto> UpdateAsync(int id, CreateUpdateCityDto input)
        {
            var existingCity = await _repository.GetAsync(id);

            if (existingCity is null)
            {
                throw new InvalidOperationException("City not found");
            }

        await CheckCityUniquness(input);
        return await base.UpdateAsync(id, input);    
        }


    public async Task<List<CityLookupDto>> GetCityLookupAsync()
    {
        using (DataFilter.Disable<IMultiTenant>())
        {

            var cities = await _repository.GetListAsync();
            var citiesList = ObjectMapper.Map<List<City>, List<CityLookupDto>>(cities)
    ;
            return citiesList;
        }

    }


    /// <summary>
    /// method used to get list of cities
    /// </summary>
    /// <param name="input">specifies max number of city and if it sorted or not</param>
    /// <returns>CityDto List</returns>
    public async Task<PagedResultDto<CityDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        // Get the IQueryable<city> from the repository
        var queryable = await _repository.GetQueryableAsync();
        // Apply search filtering
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {

            queryable = queryable.Where(c =>
              c.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
              c.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
              c.State.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower() )||
              c.State.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower())
              );
        }
        if (!input.Sorting.IsNullOrEmpty())
        {
            if (input.Sorting.Contains("DESC"))
            {
                var sorting = input.Sorting.Split(' ').ToList();
                queryable = queryable.OrderBy(sorting[0] + " descending");
            }
            else
            {
                queryable = queryable.OrderBy(input.Sorting);
            }

        }
        // Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        // Execute the query and get the list of hotel entities
        var cities = await AsyncExecuter.ToListAsync(queryable);

        // Map the hotel entities to hotelDto objects
        var cityDto = ObjectMapper.Map<List<City>, List<CityDto>>(cities);
        Console.WriteLine(cityDto.Count);
        // Return the paginated result
        return new PagedResultDto<CityDto>(
            totalCount,
            cityDto
        );
    }



    /// <summary>
    /// method used to get list of cities
    /// </summary>
    /// <param name="input">specifies max number of city and if it filtered or not</param>
    /// <returns>CityDto List</returns>
    public async Task<PagedResultDto<CityDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        // Get the IQueryable<city> from the repository
        var queryable = await _repository.GetQueryableAsync();
        // Apply filtering
        var filterExpression = BuildExpression<City>.BuildFilterExpression(input.filters);
        if (filterExpression is not null)
        {
            queryable = queryable.Where(filterExpression);
        }
        if (!input.Sorting.IsNullOrEmpty())
        {
            if (input.Sorting.Contains("DESC"))
            {
                var sorting = input.Sorting.Split(' ').ToList();
                queryable = queryable.OrderBy(sorting[0] + " descending");
            }
            else
            {
                queryable = queryable.OrderBy(input.Sorting);
            }

        }
        // Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        // Execute the query and get the list of garages entities
        var cities = await AsyncExecuter.ToListAsync(queryable);

        // Map the garages entities to garagesDto objects
        var citiesDto = ObjectMapper.Map<List<City>, List<CityDto>>(cities);
        Console.WriteLine(citiesDto.Count);
        // Return the paginated result
        return new PagedResultDto<CityDto>(
            totalCount,
            citiesDto
        );
    }

    ///<inheritdoc/>
    public override async Task DeleteAsync(int id)
    {
        var city = await _repository.FirstOrDefaultAsync(c => c.Id == id);
        if (city == null)
            throw new EntityNotFoundException(typeof(City), id);

        await _repository.HardDeleteAsync(city);

    }
    public async Task<List<CityLookupDto>> GetCityByState(int stateId)
    {
        var query = await _repository.GetQueryableAsync();
        query = query.Where(r => r.StateId == stateId);
        var cities = await AsyncExecuter.ToListAsync(query);
        var citiesList = ObjectMapper.Map<List<City>, List<CityLookupDto>>(cities);
        return citiesList;
    }
}
