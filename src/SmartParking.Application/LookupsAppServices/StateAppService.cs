
//using Abp.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Polly;
using SmartParking.Entities;
using SmartParking.Entities.Lookups;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Filters;
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
using Volo.Abp.MultiTenancy;

namespace SmartParking.LookupsAppServices;
[Authorize]

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
    #region Public Methods
    ///<inheritdoc/>
    public override async Task<StateDto> CreateAsync(CreateUpdateStateDto input)
    {
        await ValidateStateUniqunessAsync(input);
        await ValidateRegionExistsAsync(input.RegionId);
        ValidateStatus(input.Status);

        return await base.CreateAsync(input);
    }
    ///<inheritdoc/>

    public override async Task<StateDto> UpdateAsync(int id, CreateUpdateStateDto input)
    {
        var existingState = await _stateRepository.GetAsync(id)
            ?? throw new InvalidOperationException("State not found");

        await ValidateStateUniqunessAsync(input, id);
        await ValidateRegionExistsAsync(input.RegionId);
        ValidateStatus(input.Status);

        return await base.UpdateAsync(id, input);
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
    ///<inheritdoc/>

    public async Task<List<RegionLookupDto>> GetRegionLookupAsync()
    {
        using (DataFilter.Disable<IMultiTenant>())
        {

        var regions = await _regionRepository.GetListAsync();
        var regionsList = ObjectMapper.Map<List<Region>, List<RegionLookupDto>>(regions);
        return regionsList;
        }
    }
    ///<inheritdoc/>

    public async Task<List<StateLookupDto>> GetStateLookupAsync()
    {
        using (DataFilter.Disable<IMultiTenant>())
        {

        var states = await _stateRepository.GetListAsync();
        var statesList = ObjectMapper.Map<List<State>, List<StateLookupDto>>(states);
        return statesList;
        }
    }
    ///<inheritdoc/>

    public async Task<List<StateLookupDto>> GetStateLookupByRegionAsync(int regionId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
        var query = await _stateRepository.GetQueryableAsync();
        query = query.Where(x => x.RegionId == regionId);
        var states = await AsyncExecuter.ToListAsync(query);
        var statesList = ObjectMapper.Map<List<State>, List<StateLookupDto>>(states);
        return statesList;

        }
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
                c.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                c.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                c.Region.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                c.Region.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower())

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
        await _stateRepository.HardDeleteAsync(state);
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Validates the uniqueness of a state by checking if the English or Arabic names already exist for the specified region in the repository, excluding the current state (if specified by ID).
    /// </summary>
    /// <param name="input">The DTO containing the state details to validate.</param>
    /// <param name="id">The ID of the state to exclude from the check (optional).</param>
    /// <exception cref="InvalidOperationException">Thrown when the English or Arabic name of the state already exists in the repository for the given region.</exception>
    private async Task ValidateStateUniqunessAsync(CreateUpdateStateDto input, int? id = null)
    {
        string normalizedEnglish = input.EnglishName?.Trim().ToLowerInvariant();
        string normalizedArabic = input.ArabicName?.Trim().ToLowerInvariant();

        var state = await _stateRepository.FirstOrDefaultAsync(s =>
            s.RegionId == input.RegionId &&
            (s.EnglishName.ToLower() == normalizedEnglish ||
             s.ArabicName.ToLower() == normalizedArabic) &&
            (!id.HasValue || s.Id != id.Value));

        if (state == null) return;

        if (state.EnglishName.Trim().ToLowerInvariant() == normalizedEnglish)
            throw new InvalidOperationException("State english name already exists");

        if (state.ArabicName.Trim().ToLowerInvariant() == normalizedArabic)
            throw new InvalidOperationException("State arabic name already exists");
    }

    /// <summary>
    /// Validates if a region with the specified ID exists in the repository.
    /// </summary>
    /// <param name="regionId">The ID of the region to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when the region with the given ID does not exist.</exception>
    private async Task ValidateRegionExistsAsync(int regionId)
    {
        var region = await _regionRepository.FindAsync(regionId);
        if (region is null)
            throw new InvalidOperationException("Region does not exist");
    }

    /// <summary>
    /// Validates that the given status is one of the valid status values: Active, Inactive, or Deleted.
    /// </summary>
    /// <param name="status">The status to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when the status is invalid.</exception>
    private void ValidateStatus(Status status)
    {
        if (status != Status.Active && status != Status.Inactive && status != Status.Deleted)
            throw new InvalidOperationException("Invalid status value.");
    }
    #endregion
}
