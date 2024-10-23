using Microsoft.AspNetCore.Authorization;
using SmartParking.Entities;
using SmartParking.Entities.Cameras;
using SmartParking.Lookups;
using SmartParking.Lookups.City;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.LookupsAppServices;
[Authorize]


public class CitiesAppService : CrudAppService<City, CityDto,
                                int, PagedAndSortedResultRequestDto,
                                CreateUpdateCityDto>,
    ICitiesAppService

{
    private readonly IRepository<City, int> _repository;
    private readonly IRepository<Governorate, int> _governorateRepository;

    public CitiesAppService(IRepository<City, int> repository, IRepository<Governorate, int> governorateRepository)
    : base(repository)
    {
        _repository = repository;
        _governorateRepository = governorateRepository;
    }

    ///<inheritdoc/>
    public override async Task<CityDto> CreateAsync(CreateUpdateCityDto input)
    {
        var city = await _repository.FirstOrDefaultAsync
            (city =>
            city.EnglishName == input.EnglishName ||
            city.ArabicName == input.ArabicName);

        if (city != null)
        {
            if (city.EnglishName == input.EnglishName)
            {
                throw new InvalidOperationException("city english name already exists");

            }
            else if (city.ArabicName == input.ArabicName)
            {
                throw new InvalidOperationException("city arabic name already exists");

            }

            throw new InvalidOperationException("city english name , city arabic name already exist");
        }
        else
        {
            return await base.CreateAsync(input);
        }
    }
    ///<inheritdoc/>
    public override async Task<CityDto> UpdateAsync(int id, CreateUpdateCityDto input)
    {
        var city = await _repository.FirstOrDefaultAsync
            (city =>
            city.EnglishName == input.EnglishName ||
            city.ArabicName == input.ArabicName);

        if (city != null)
        {
            if(city.EnglishName == input.EnglishName)
            {
                throw new InvalidOperationException("city english name already exists");

            }
            else if(city.ArabicName == input.ArabicName)
            {
                throw new InvalidOperationException("city arabic name already exists");

            }

            throw new InvalidOperationException("city english name , city arabic name already exist");
        }
        else
        {
            return await base.UpdateAsync(id, input);
        }
    }
    public async Task<List<GovernorateLookupDto>> GetGovernorateLookupAsync()
    {
        var governorates = await _governorateRepository.GetListAsync();
        var governoratesList = ObjectMapper.Map<List<Governorate>, List<GovernorateLookupDto>>(governorates)
;
        return governoratesList;

    }

    public async Task<List<CityLookupDto>> GetCityLookupAsync()
    {
        var cities = await _repository.GetListAsync();
        var citiesList = ObjectMapper.Map<List<City>, List<CityLookupDto>>(cities)
;
        return citiesList;

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
                c.ArabicName.StartsWith(input.SearchingTerm) ||
                c.EnglishName.StartsWith(input.SearchingTerm)

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
        var city = await GetAsync(id);
        if (city == null)
            throw new EntityNotFoundException(typeof(City), id);

        await base.DeleteAsync(id);
    }

}
