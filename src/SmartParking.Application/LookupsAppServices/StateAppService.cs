
//using Abp.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Polly;
using SmartParking.Entities;
using SmartParking.Entities.Lookups;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Enums;
using SmartParking.Lookups;
using SmartParking.Lookups.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.LookupsAppServices;
[Authorize]

public class StateAppService : CrudAppService<State, StateDto,
									int, PagedAndSortedResultRequestDto,
									CreateUpdateStateDto>,
		IStateAppService
{

    private readonly IRepository<Region, int> _regionRepository;
    private readonly IRepository<State, int> _stateRepository;
    private readonly ICascadeSoftDeleteHandler<State> _deleteHandler;
    private readonly IDataFilter _dataFilter;


    public StateAppService(IRepository<State, int> stateRepository,ICascadeSoftDeleteHandler<State> deleteHandler, IRepository<Region, int> regionRepository,IDataFilter dataFilter)
	: base(stateRepository)
    {
        _dataFilter = dataFilter;
        _stateRepository = stateRepository;
        this._deleteHandler = deleteHandler;
        _regionRepository = regionRepository;
    }

    ///<inheritdoc/>

    public override async Task<StateDto> CreateAsync(CreateUpdateStateDto input)
    {
        var state = await _stateRepository.FirstOrDefaultAsync
                (state =>
                state.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
                state.ArabicName.ToLower().Contains(input.ArabicName.ToLower()));

        var exisitingRegion = await _regionRepository.FindAsync(input.RegionId);

        if (exisitingRegion is null)
        {
            throw new InvalidOperationException("region doesnot exist");
        }

        if (state != null)
        {
            if (state.EnglishName == input.EnglishName)
            {
                throw new InvalidOperationException("state english name already exists");

            }
            else if (state.ArabicName == input.ArabicName)
            {
                throw new InvalidOperationException("state arabic name already exists");

            }

            throw new InvalidOperationException("english name , arabic name already exist");
        }

        if (input.Status != Status.Active && input.Status != Status.Inactive && input.Status != Status.Deleted)
        {
            throw new InvalidOperationException("Invalid status value.");

        }
        
        
            return await base.CreateAsync(input);
    }

    ///<inheritdoc/>
    public override async Task<StateDto> UpdateAsync(int id, CreateUpdateStateDto input)
    {
        var exisitingState = await _stateRepository.GetAsync(id);
        if (exisitingState is null)
        {
            throw new InvalidOperationException("State not found");
        }
        var exisitingRegion = await _regionRepository.FindAsync(input.RegionId);

        if (exisitingRegion is null)
        {
            throw new InvalidOperationException("region doesnot exist");
        }


        var state = await _stateRepository.FirstOrDefaultAsync
                (state =>
                state.RegionId == input.RegionId &&
                state.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
                state.ArabicName.ToLower().Contains(input.ArabicName.ToLower()));

        if (state != null && state.Id != id )
        {
            if (state.EnglishName == input.EnglishName.Trim(' ') )
            {
                throw new InvalidOperationException("state english name already exists");

            }
            else if (state.ArabicName == input.ArabicName.Trim(' '))
            {
                throw new InvalidOperationException("state arabic name already exists");

            }

          
        }

        if (input.Status != Status.Active && input.Status != Status.Inactive && input.Status != Status.Deleted)
        {
            throw new InvalidOperationException("Invalid status value.");

        }

       

        return await base.UpdateAsync(id,input);
    }

    ///<inheritdoc/>

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

    public async Task<List<StateLookupDto>> GetStateLookupByRegionAsync(int regionId)
    {
        var query = await _stateRepository.GetQueryableAsync();
        query = query.Where(x => x.RegionId == regionId);
        var states = await AsyncExecuter.ToListAsync(query);
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
                c.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                c.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower())

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

    ///<inheritdoc/>
    public override async Task DeleteAsync(int id)
    {
        var state = await _stateRepository.FindAsync(id);

        if (state is null)
        {
            throw new EntityNotFoundException(typeof(State), id);
        }
        
           // await base.DeleteAsync(id);

        await _stateRepository.DeleteAsync(state);
        // Manually publish the EntityDeletedEto event
        var deleteEvent = new EntityDeletedEto<State>(state);
        await _deleteHandler.HandleEventAsync(deleteEvent);
    }
}
