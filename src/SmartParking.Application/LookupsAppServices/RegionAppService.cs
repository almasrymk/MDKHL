using AutoMapper.Internal.Mappers;
using SmartParking.Entities;
using SmartParking.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using SmartParking.Entities.Lookups;
using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;
using SmartParking.Lookups.Region;
using Volo.Abp.Domain.Repositories;
using SmartParking.Enums;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace SmartParking.LookupsAppServices;
[Authorize]

public class RegionAppService : CrudAppService<Region, RegionDto,
									int, PagedAndSortedResultRequestDto,
									CreateUpdateRegionDto>,
		IRegionAppService
{
	public readonly IRepository<Region, int> _regionRepository;
    public readonly IRepository<Country, int> _countryRepository;

    public readonly IDataFilter _dataFilter;

    public RegionAppService(IRepository<Region, int> regionRepository,IDataFilter dataFilter, IRepository<Country, int> countryRepository)
	: base(regionRepository)
	{
		_regionRepository = regionRepository;
        _dataFilter = dataFilter;
        _countryRepository = countryRepository;
	}

    ///<inheritdoc/>

    public override async Task<RegionDto> CreateAsync(CreateUpdateRegionDto input)
    {
        var region = await _regionRepository.FirstOrDefaultAsync
                (region =>
                region.EnglishName == input.EnglishName ||
                region.ArabicName == input.ArabicName);

        var exisitingCountry = await _countryRepository.FindAsync(input.CountryId);

        if (region != null)
        {
            if (region.EnglishName == input.EnglishName)
            {
                throw new InvalidOperationException("region english name already exists");

            }
            else if (region.ArabicName == input.ArabicName)
            {
                throw new InvalidOperationException("region arabic name already exists");

            }

            throw new InvalidOperationException("english name , arabic name already exist");
        }

        if(exisitingCountry is null)
        {
            throw new InvalidOperationException("country doesnot exist");
        }

        if (input.Status != Status.Active && input.Status != Status.Inactive && input.Status != Status.Deleted)
        {
            throw new InvalidOperationException("Invalid status value.");

        }
        
        else
            return await base.CreateAsync(input);
    }

    ///<inheritdoc/>
    public override async Task<RegionDto> UpdateAsync(int id, CreateUpdateRegionDto input)
    {
        var region = await _regionRepository.FirstOrDefaultAsync
                (region =>
                region.EnglishName == input.EnglishName ||
                region.ArabicName == input.ArabicName);

        var exisitingCountry = await _countryRepository.FindAsync(input.CountryId);

        if (region != null)
        {
            if (region.EnglishName == input.EnglishName)
            {
                throw new InvalidOperationException("region english name already exists");

            }
            else if (region.ArabicName == input.ArabicName)
            {
                throw new InvalidOperationException("region arabic name already exists");

            }

            throw new InvalidOperationException("english name , arabic name already exist");
        }

        if (exisitingCountry is null)
        {
            throw new InvalidOperationException("country doesnot exist");
        }

        if (input.Status != Status.Active && input.Status != Status.Inactive && input.Status != Status.Deleted)
        {
            throw new InvalidOperationException("Invalid status value.");

        }
       
        else
            return await base.UpdateAsync(id,input);
    }

    /// <summary>
    /// method used to get list of regions
    /// </summary>
    /// <param name="input">specifies max number of region and if it filtered or not</param>
    /// <returns>RegionDto List</returns>

    public async Task<PagedResultDto<RegionDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        // Get the IQueryable<state> from the repository
        var queryable = await _regionRepository.GetQueryableAsync();
        // Apply filtering
        var filterExpression = BuildExpression<Region>.BuildFilterExpression(input.filters);
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
        var regions = await AsyncExecuter.ToListAsync(queryable);

        // Map the garages entities to garagesDto objects
        var regionDto = ObjectMapper.Map<List<Region>, List<RegionDto>>(regions);
        Console.WriteLine(regionDto.Count);
        // Return the paginated result
        return new PagedResultDto<RegionDto>(
            totalCount,
            regionDto
        );
    }

    /// <summary>
    /// method used to get list of regions
    /// </summary>
    /// <param name="input">specifies max number of region and if it filtered or not</param>
    /// <returns>RegionDto List</returns>
    public async Task<PagedResultDto<RegionDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        // Get the IQueryable<state> from the repository
        var queryable = await _regionRepository.GetQueryableAsync();
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
        var regions = await AsyncExecuter.ToListAsync(queryable);

        // Map the hotel entities to hotelDto objects
        var regionDto = ObjectMapper.Map<List<Region>, List<RegionDto>>(regions);
        Console.WriteLine(regionDto.Count);
        // Return the paginated result
        return new PagedResultDto<RegionDto>(
            totalCount,
            regionDto
        );
    }

    ///<inheritdoc/>
    public override async Task DeleteAsync(int id)
    {
        var city = await GetAsync(id);
        if (city == null)
            throw new EntityNotFoundException(typeof(Region), id);

        await base.DeleteAsync(id);
    }

}
