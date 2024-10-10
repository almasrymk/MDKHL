
using AutoMapper.Internal.Mappers;
using Microsoft.EntityFrameworkCore;
using SmartParking.Bases;
using SmartParking.Cameras;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Organizations;
using SmartParking.Garages;
using SmartParking.Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace SmartParking.DefaultPackages;

public class PackagesAppService : CrudAppService<
            Package,
            PackageDto,
            int,
            PagedAndSortedResultRequestDto,
            CreateUpdatePackageDto
            >, IPackagesAppService
{
		private readonly IRepository<Package, int> _repository;

        public PackagesAppService(IRepository<Package, int> repository):base(repository)
        {
            _repository = repository;
        }


  //      public async Task<PackageDto> CreateAsync(CreateUpdatePackageDto input)
		//{
		//return await base.CreateAsync(input);
		////var validationResult = new CreateUpdatePackageDtoValidator().Validate(input);
		////if (!validationResult.IsValid)
		////{
		////	var exception = GetValidationException(validationResult);
		////	throw exception;
		////}
		////var package = ObjectMapper.Map<CreateUpdatePackageDto, Package>(input);
		////var inserted = await _repository.InsertAsync(package, autoSave: true);
		////return ObjectMapper.Map<Package, PackageDto>(inserted);
		//}


      

    //public Task<PagedResultDto<PackageDto>> GetbyOrganizationAsync(PackageListDto input, int OrgnizationId)
    //{
    //	throw new NotImplementedException();
    //}
    //public override Task<PagedResultDto<PackageDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    //{
    //    return base.GetListAsync(input);
    //}

    //public async Task<PagedResultDto<PackageDto>> GetListAsync(PackageListDto input)
    //{	
    //	return await base.GetListAsync(input);
    //	//if (input.Sorting.IsNullOrWhiteSpace())
    //	//	input.Sorting = nameof(Package.Id);
    //	//var packages = await _repository.WithDetailsAsync(_ => _.Organization)
    //	//													 .Result
    //	//													 .AsQueryable()
    //	//													 .WhereIf(!input.Filter.IsNullOrWhiteSpace()
    //	//															  , _ => _.Organization.ArabicName.Contains(input.Filter)
    //	//																  || _.Organization.EnglishName.Contains(input.Filter))
    //	//													 .Skip(input.SkipCount)
    //	//													 .Take(input.MaxResultCount)
    //	//													 .OrderBy(input.Sorting)
    //	//													 .ToListAsync();
    //	//var totalCount = input.Filter == null
    //	//	? await _repository.CountAsync()
    //	//	: await _repository.CountAsync(_ => _.Organization.ArabicName.Contains(input.Filter)
    //	//												   || _.Organization.EnglishName.Contains(input.Filter));

    //	//return new PagedResultDto<PackageDto>(
    //	//	totalCount,
    //	//	ObjectMapper.Map<List<Package>, List<PackageDto>>(packages));
    //}
    ///<inheritdoc>/>
    //[RemoteService(IsEnabled = false)]
    public async Task<List<PackageDto>> GetListPackageByOrganizationId(int organizationId)
    {
		var query = await _repository.GetQueryableAsync();
		var pakages =await query.Where(_ => _.OrgnizationId == organizationId).ToListAsync();
		var pakagesMap = ObjectMapper.Map<List<Package>, List<PackageDto>>(pakages);
		return pakagesMap;
    }


  //      public async Task<PackageDto> UpdateAsync(int id, CreateUpdatePackageDto input)
	 //   {
		//return await base.UpdateAsync(id, input);
	 //   }
        public async Task<PagedResultDto<PackageDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
        {
        // Get the IQueryable<camera> from the repository
        var queryable = await _repository.GetQueryableAsync();
        // Apply search filtering
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {

            queryable = queryable.Where(or =>
                or.ArabicName.StartsWith(input.SearchingTerm) ||
                or.CreatedBy.StartsWith(input.SearchingTerm) ||
                or.DeactivatedBy.StartsWith(input.SearchingTerm) ||
                or.Description.StartsWith(input.SearchingTerm) ||
                or.EnglishName.StartsWith(input.SearchingTerm) 
                
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
		query.Where(x => x.OrgnizationId == organizationId);
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
}
