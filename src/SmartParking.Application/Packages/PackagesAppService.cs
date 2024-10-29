
using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SmartParking.Bases;
using SmartParking.Cameras;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Organizations;
using SmartParking.Garages;
using SmartParking.Packages;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;

namespace SmartParking.DefaultPackages;

[Authorize]
public class PackagesAppService : CrudAppService<
            Package,
            PackageDto,
            int,
            PagedAndSortedResultRequestDto,
            CreateUpdatePackageDto
            >, IPackagesAppService
{
		private readonly IRepository<Package, int> _repository;
    private readonly IDataFilter _softDeleteFilter;

    public PackagesAppService(IRepository<Package, int> repository, IDataFilter softDeleteFilter) :base(repository)
    {
            _repository = repository;
            _softDeleteFilter = softDeleteFilter;
    }


  
    public async Task<Result<List<PackageDto>>> GetListPackageByOrganizationId(int organizationId)
    {
        using (_softDeleteFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            var pakages = await query.Where(_ => _.OrganizationId == organizationId).ToListAsync();
            var pakagesMap = ObjectMapper.Map<List<Package>, List<PackageDto>>(pakages);
            if (pakages.Count == 0)
            {
                return Result<List<PackageDto>>.NotFound();
            }
            return Result<List<PackageDto>>.Success(pakagesMap);
        }
         
    }
        public async Task<PagedResultDto<PackageDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
        {
        // Get the IQueryable<camera> from the repository
        var queryable = await _repository.GetQueryableAsync();
        // Apply search filtering
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {

            queryable = queryable.Where(or =>
                or.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.CreatedBy.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.DeactivatedBy.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Description.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) 
                
            );
        }
        // Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        // Execute the query and get the list of camera entities
        var packages = await AsyncExecuter.ToListAsync(queryable);

        // Map the hotel entities to cameraDto objects
        var packagesDto = ObjectMapper.Map<List<Package>, List<PackageDto>>(packages);
        // Return the paginated result
        return new PagedResultDto<PackageDto>(
            totalCount,
            packagesDto
        );
    }
        public async Task<PagedResultDto<PackageDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
        {
        // Get the IQueryable<camera> from the repository
        var queryable = await _repository.GetQueryableAsync();
        // Apply filtering
        var filterExpression = BuildExpression<Package>.BuildFilterExpression(input.filters);
        if (filterExpression is not null)
        {
            queryable = queryable.Where(filterExpression);
        }
        // Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        // Execute the query and get the list of organizations entities
        var packages = await AsyncExecuter.ToListAsync(queryable);

        // Map the organizations entities to organizationsDto objects
        var packagesDto = ObjectMapper.Map<List<Package>, List<PackageDto>>(packages);
        // Return the paginated result
        return new PagedResultDto<PackageDto>(
            totalCount,
            packagesDto
        );
    }
	public async Task<PagedResultDto<PackageDto>> GetPackagesByOrganization(PagedAndSortedResultRequestDto input, int organizationId)
	{
		var query = await _repository.GetQueryableAsync();
		query.Where(x => x.OrganizationId == organizationId);
		var totalCount = await AsyncExecuter.CountAsync(query);
		query = query.Skip(input.SkipCount).Take(input.MaxResultCount);
		query.OrderBy(x => x.EnglishName);
		var garages = await AsyncExecuter.ToListAsync(query);
		var garagesDto = ObjectMapper.Map<List<Package>, List<PackageDto>>(garages);
		return new PagedResultDto<PackageDto>(
		   totalCount,
		   garagesDto
	   );
	}
    public override async Task DeleteAsync(int id)
    {
        var package = await _repository.FindAsync(id);

        if (package is null)
        {
            throw new EntityNotFoundException(typeof(VehicleModel), id);
        }


        await base.DeleteAsync(id);
    }
}
