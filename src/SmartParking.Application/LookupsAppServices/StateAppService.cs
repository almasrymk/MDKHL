
using SmartParking.Entities;
using SmartParking.Entities.Lookups;
using SmartParking.Lookups;
using SmartParking.Lookups.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.LookupsAppServices;

public class StateAppService : CrudAppService<State, StateDto,
									int, PagedAndSortedResultRequestDto,
									CreateUpdateStateDto>,
		IStateAppService
{

    private readonly IRepository<Region, int> _regionRepository;
    private readonly IRepository<State, int> _stateRepository;

    public StateAppService(IRepository<State, int> stateRepository, IRepository<Region, int> regionRepository)
	: base(stateRepository)
    {
        _stateRepository = stateRepository;
        _regionRepository = regionRepository;
    }


    /// <summary>
    /// method used to get list of states
    /// </summary>
    /// <param name="input">specifies max number of state and if it filtered or not</param>
    /// <returns>StateDto List</returns>
    public async Task<PagedResultDto<StateDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        // Get the IQueryable<garage> from the repository
        var queryable = await _stateRepository.GetQueryableAsync();
        // Apply filtering
        var filterExpression = BuildExpression<State>.BuildFilterExpression(input.filters);
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
        var states = await AsyncExecuter.ToListAsync(queryable);

        // Map the garages entities to garagesDto objects
        var stateDto = ObjectMapper.Map<List<State>, List<StateDto>>(states);
        Console.WriteLine(stateDto.Count);
        // Return the paginated result
        return new PagedResultDto<StateDto>(
            totalCount,
            stateDto
        );
    }

    public async Task<List<RegionLookupDto>> GetRegionLookupAsync()
    {
        var regions = await _regionRepository.GetListAsync();
        var regionsList = ObjectMapper.Map<List<Region>, List<RegionLookupDto>>(regions);
        return regionsList;
    }

    public async Task<List<StateLookupDto>> GetStateLookupAsync()
    {
        var states = await _stateRepository.GetListAsync();
        var statesList = ObjectMapper.Map<List<State>, List<StateLookupDto>>(states);
        return statesList;
    }

    /// <summary>
    /// method used to get list of states
    /// </summary>
    /// <param name="input">specifies max number of state and if it filtered or not</param>
    /// <returns>StateDto List</returns>
    public async Task<PagedResultDto<StateDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        // Get the IQueryable<state> from the repository
        var queryable = await _stateRepository.GetQueryableAsync();
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
        var states = await AsyncExecuter.ToListAsync(queryable);

        // Map the hotel entities to hotelDto objects
        var stateDto = ObjectMapper.Map<List<State>, List<StateDto>>(states);
        Console.WriteLine(stateDto.Count);
        // Return the paginated result
        return new PagedResultDto<StateDto>(
            totalCount,
            stateDto
        );
    }
}
