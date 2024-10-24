using SmartParking.Entities;
using SmartParking.Lookups;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace SmartParking.LookupsAppServices;
[Authorize]

public class PackageTypesAppService : CrudAppService<PackageType, PackageTypeDto,
                                int, PagedAndSortedResultRequestDto,
                                CreateUpdatePackageTypeDto>,
    IPackageTypesAppService

{
    public readonly IRepository<PackageType, int> _packageTypeRepository;
    public PackageTypesAppService(IRepository<PackageType, int> packageTypeRepository) 
    : base(packageTypeRepository)
    {
        _packageTypeRepository = packageTypeRepository;
    }

        /// <inheritdoc/>

        public override async Task<PackageTypeDto> CreateAsync(CreateUpdatePackageTypeDto input)
        {
            var packageType = await _packageTypeRepository.FirstOrDefaultAsync
                (type =>
                type.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
                type.ArabicName.ToLower().Contains(input.ArabicName.ToLower()));
         

            if (packageType != null)
            {
                if (packageType.EnglishName == input.EnglishName.Trim(' ') && packageType.Id != input.Id)
                {
                    throw new InvalidOperationException("package english name already exists");

                }
                else if (packageType.ArabicName == input.ArabicName.Trim(' ') && packageType.Id != input.Id)
                {
                    throw new InvalidOperationException("Package arabic name already exists");

                }
           

            }
               return await base.CreateAsync(input);
        }

        /// <inheritdoc/>
        public override async Task<PackageTypeDto> UpdateAsync(int id, CreateUpdatePackageTypeDto input)
        {
            var existingPackage= await _packageTypeRepository.GetAsync(id);

            if (existingPackage is null)
            {
                throw new InvalidOperationException("Package Type not found");
            }

            var packageType = await _packageTypeRepository.FirstOrDefaultAsync
                    (type =>
                    type.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
                    type.ArabicName.ToLower().Contains(input.ArabicName.ToLower()));


            if (packageType != null)
            {
                if (packageType.EnglishName == input.EnglishName.Trim(' ') && packageType.Id != input.Id)
                {
                    throw new InvalidOperationException("package english name already exists");

                }
                else if (packageType.ArabicName == input.ArabicName.Trim(' ') && packageType.Id != input.Id)
                {
                    throw new InvalidOperationException("Package arabic name already exists");

                }


            }
                return await base.UpdateAsync(id,input);
        }
        /// <summary>
        /// method used to get list of package Types
        /// </summary>
        /// <param name="input">specifies max number of package Type and if it filtered or not</param>
        /// <returns>PackageTypeDto List</returns>
        public async Task<PagedResultDto<PackageTypeDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
        {
            // Get the IQueryable<garage> from the repository
            var queryable = await _packageTypeRepository.GetQueryableAsync();
            // Apply filtering
            var filterExpression = BuildExpression<PackageType>.BuildFilterExpression(input.filters);
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
        var packageTypesDto = await AsyncExecuter.ToListAsync(queryable);

        // Map the garages entities to garagesDto objects
        var packageTypeDto = ObjectMapper.Map<List<PackageType>, List<PackageTypeDto>>(packageTypesDto);
        Console.WriteLine(packageTypeDto.Count);
        // Return the paginated result
        return new PagedResultDto<PackageTypeDto>(
            totalCount,
            packageTypeDto
        );
    }

    /// <summary>
    /// method used to get list of package types
    /// </summary>
    /// <param name="input">specifies max number of package type and if it filtered or not</param>
    /// <returns>PackageTypeDto List</returns>
    public async Task<PagedResultDto<PackageTypeDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        // Get the IQueryable<garage> from the repository
        var queryable = await _packageTypeRepository.GetQueryableAsync();
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
        var packageTypes = await AsyncExecuter.ToListAsync(queryable);

        // Map the hotel entities to hotelDto objects
        var packageTypeDto = ObjectMapper.Map<List<PackageType>, List<PackageTypeDto>>(packageTypes);
        Console.WriteLine(packageTypeDto.Count);
        // Return the paginated result
        return new PagedResultDto<PackageTypeDto>(
            totalCount,
            packageTypeDto
        );
    }


    ///<inheritdoc/>
    public override async Task DeleteAsync(int id)
    {
        var city = await GetAsync(id);
        if (city == null)
            throw new EntityNotFoundException(typeof(PackageType), id);

        await base.DeleteAsync(id);
    }

}
