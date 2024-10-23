
using SmartParking.Entities;
using SmartParking.Entities.Lookups;
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
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.LookupsAppServices;

public class StateAppService : CrudAppService<State, StateDto,
									int, PagedAndSortedResultRequestDto,
									CreateUpdateStateDto>,
		IStateAppService
{

    private readonly IRepository<Region, int> _regionRepository;
    private readonly IRepository<State, int> _stateRepository;
    private readonly IDataFilter _dataFilter;


    public StateAppService(IRepository<State, int> stateRepository, IRepository<Region, int> regionRepository,IDataFilter dataFilter)
	: base(stateRepository)
    {
        _dataFilter = dataFilter;
        _stateRepository = stateRepository;
        _regionRepository = regionRepository;
    }

    ///<inheritdoc/>

    public override async Task<StateDto> CreateAsync(CreateUpdateStateDto input)
    {
        var state = await _regionRepository.FirstOrDefaultAsync
                (region =>
                region.EnglishName == input.EnglishName ||
                region.ArabicName == input.ArabicName);

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
        
        else
            return await base.CreateAsync(input);
    }

    ///<inheritdoc/>
    public override async Task<StateDto> UpdateAsync(int id, CreateUpdateStateDto input)
    {
        var state = await _regionRepository.FirstOrDefaultAsync
                (region =>
                region.EnglishName == input.EnglishName ||
                region.ArabicName == input.ArabicName);

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
       
        else
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

    ///<inheritdoc/>

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

    ///<inheritdoc/>
    public override async Task DeleteAsync(int id)
    {
        var state = await base.GetAsync(id);

        if (state is null)
        {
            throw new EntityNotFoundException(typeof(State), id);
        }
        else
            await base.DeleteAsync(id);

        
    }
}
