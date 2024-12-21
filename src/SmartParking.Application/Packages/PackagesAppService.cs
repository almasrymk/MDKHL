using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Organizations;
using SmartParking.Packages;
using SmartParking.Permissions.PackagePermissions;
using SmartParking.Results;
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
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IDataFilter _dataFilter;
    private readonly IOrganizationsAppService _organizations;

    public PackagesAppService(IRepository<Package, int> repository, IRepository<Customer, Guid> customerRepository, IDataFilter dataFilter, IOrganizationsAppService organizations) : base(repository)
    {
        _repository = repository;
        _customerRepository = customerRepository;
        _dataFilter = dataFilter;
        _organizations = organizations;
        GetPolicyName = PackagePermissions.Default;
        GetListPolicyName = PackagePermissions.Default;
        CreatePolicyName = PackagePermissions.Create;
        UpdatePolicyName = PackagePermissions.Edit;
        DeletePolicyName = PackagePermissions.Delete;

    }



    public async Task<Result<List<PackageDto>>> GetListPackageByOrganizationId(int organizationId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var customer = await _customerRepository.FirstOrDefaultAsync(_ => _.IdentityUserId == CurrentUser.Id);
            var query = await _repository.GetQueryableAsync();
            var pakages = await query.Where(_ => _.OrganizationId == organizationId &&
            _.PackageStatus == SubscriptionStatus.Active && _.CustomerCategory == CustomerCategoryStatus.Customer).ToListAsync();
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
                or.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.PackageType.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.PackageType.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Price.ToString().ToLower().Contains(input.SearchingTerm.ToLower())
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

        package.Status = Status.Deleted;
        await CurrentUnitOfWork.SaveChangesAsync();
        //await base.DeleteAsync(id);
    }
    ///<inheritdoc/>
    public override async Task<PagedResultDto<PackageDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {

        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryable = await _repository.GetQueryableAsync();
            queryable = queryable.Include(p => p.PackageType);
            queryable = queryable.Include(p => p.CustomerPackages);

            queryable = queryable.Where(p => p.TenantId == CurrentTenant.Id);

            if (!string.IsNullOrEmpty(input.Sorting))
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
            var totalCount = await AsyncExecuter.CountAsync(queryable);

            queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
            var packages = await AsyncExecuter.ToListAsync(queryable);
            var packagesDto = ObjectMapper.Map<List<Package>, List<PackageDto>>(packages);

            return new PagedResultDto<PackageDto>(
                totalCount,
                packagesDto
            );
        }
    }
    ///<inheritdoc/>
    public async Task<List<PackageDto>> GetAllMdkhlAsync(int organizationId, CustomerCategoryStatus customerCategory)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

            var queryable = await _repository.GetQueryableAsync();
            queryable = queryable.Include(p => p.PackageType);
            queryable = queryable.Where(x => x.OrganizationId == organizationId && x.CustomerCategory == customerCategory);
            var totalCount = await AsyncExecuter.CountAsync(queryable);
            var packages = await AsyncExecuter.ToListAsync(queryable);
            var packagesDto = ObjectMapper.Map<List<Package>, List<PackageDto>>(packages);

            return packagesDto;


        }
    }
    ///<inheritdoc/>
    public async Task<List<PackageDto>> GetAllOrganizationAsync(CustomerCategoryStatus customerCategory)
    {
        var organization = await _organizations.GetOrganizationForCurrentTenat();
        var queryable = await _repository.GetQueryableAsync();
        queryable = queryable.Include(p => p.PackageType);
        queryable = queryable.Where(x => x.CustomerCategory == customerCategory && x.OrganizationId == organization.Id);
        var totalCount = await AsyncExecuter.CountAsync(queryable);
        List<Package> packages;
        using (_dataFilter.Disable<IMultiTenant>())
        {
            packages = await AsyncExecuter.ToListAsync(queryable);
        }
        var packagesDto = ObjectMapper.Map<List<Package>, List<PackageDto>>(packages);

        return packagesDto;
    }
}
