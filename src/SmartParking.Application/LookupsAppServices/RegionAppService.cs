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

namespace SmartParking.LookupsAppServices;

public class RegionAppService : CrudAppService<Region, RegionDto,
									int, PagedAndSortedResultRequestDto,
									CretaeUpdateRegionDto>,
		IRegionAppService
{
	public readonly IRepository<Region, int> _regionRepository;
    public RegionAppService(IRepository<Region, int> regionRepository)
	: base(regionRepository)
	{
		_regionRepository = regionRepository;
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
}
