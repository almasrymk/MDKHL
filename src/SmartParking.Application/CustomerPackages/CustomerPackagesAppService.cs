using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartParking.CustomerPackages.Dtos;
using SmartParking.CustomerPackages.Inputs;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using SmartParking.Organizations;
using SmartParking.Payments;
using SmartParking.Results;
using SmartParking.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;


namespace SmartParking.CustomerPackages;
//[Authorize]
public class CustomerPackagesAppService :
     CrudAppService<
        CustomerPackage,
        CustomerPackageDto,
        int,
        PagedAndSortedResultRequestDto,
        CreateUpdateCustomerPackageDto
        >,
    ICustomerPackagesAppService
{
    private readonly IRepository<CustomerPackage, int> _repository;
    private readonly IDataFilter _dataFilter;
    private readonly IOrganizationsAppService _organizationsApp;
    private readonly IRepository<Vehicle, int> _vehicleRepository;

    public CustomerPackagesAppService(IRepository<CustomerPackage, int> repository,
        IDataFilter dataFilter, 
        IOrganizationsAppService organizationsApp, 
        IRepository<Vehicle, int> vehicleRepository
  ) : base(repository)
    {
        _repository = repository;
        _dataFilter = dataFilter;
        _organizationsApp = organizationsApp;
        _vehicleRepository = vehicleRepository;
    }
    #region public methods

    ///<inheritdoc/>
    public async Task<Result<List<CustomerPackageDto>>> GetPackageByCustomer()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Where(c => c.Customer.IdentityUserId == CurrentUser.Id);

            var result = await AsyncExecuter.ToListAsync(query);

            var packageMap = ObjectMapper.Map<List<CustomerPackage>, List<CustomerPackageDto>>(result);
            return Result<List<CustomerPackageDto>>.Success(packageMap);
        }
    }

    public async Task AddDefaultPackageToCustomerAsync(DefaultCustomerPackageDto defaultCustomerPackageDto)
    {
        await _repository.InsertAsync(new CustomerPackage
        {
            CustomerId = defaultCustomerPackageDto.CustomerId,
            OrganizationId = defaultCustomerPackageDto.OrganizationId,
            TenantId = defaultCustomerPackageDto.TenantId,
        });
    }

    ///<inheritdoc/>
    public async Task<PagedResultDto<CustomerPackageDto>> GetListPackagesByCustomerMdkl(Guid customerId, PagedAndSortedResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Where(c => c.CustomerId == customerId && c.ArPlateNumber != null && c.EnPlateNumber != null);
            var resultPaged = await ApplyPaged(input, query);
            var result = await AsyncExecuter.ToListAsync(resultPaged.query);
            var packageMap = ObjectMapper.Map<List<CustomerPackage>, List<CustomerPackageDto>>(result);
            return new PagedResultDto<CustomerPackageDto>
            {
                Items = packageMap,
                TotalCount = resultPaged.totalCount
            };
        }
    }
    ///<inheritdoc/>
    public async Task<PagedResultDto<CustomerPackageDto>> GetListPackagesByCustomerOrganization(Guid customerId, PagedAndSortedResultRequestDto input)
    {
        var org = await _organizationsApp.GetOrganizationForCurrentTenatAsync();
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Where(c => c.CustomerId == customerId && c.OrganizationId == org.Id && c.ArPlateNumber != null && c.EnPlateNumber!=null );
            var resultPaged = await ApplyPaged(input, query);
            var result = await AsyncExecuter.ToListAsync(resultPaged.query);
            var packageMap = ObjectMapper.Map<List<CustomerPackage>, List<CustomerPackageDto>>(result);
            return new PagedResultDto<CustomerPackageDto>
            {
                Items = packageMap,
                TotalCount = resultPaged.totalCount
            };

        }
    }

    public override async Task<CustomerPackageDto> GetAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var customerPackage = await base.GetAsync(id);
            var customerID = customerPackage.CustomerId;
            var vehicle = await _vehicleRepository.FirstOrDefaultAsync(v => v.CustomerId == customerID);
            customerPackage.LicenseTypeImage = vehicle?.LicenceType?.Image;
            customerPackage.VehicleModelImage = vehicle?.VehicleModel?.Image;
            return customerPackage;
        }
    }
    #endregion

    #region private methods 
    /// <summary>
    /// Applies pagination to a given <see cref="IQueryable{CustomerPackage}"/> based on the specified input parameters.
    /// </summary>
    /// <param name="input">Pagination and sorting input data.</param>
    /// <param name="query">The query to apply pagination to.</param>
    /// <returns>
    /// A tuple containing the modified query with pagination applied, and the total count of items before pagination.
    /// </returns>
    private async Task<(IQueryable<CustomerPackage> query, int totalCount)> ApplyPaged(PagedAndSortedResultRequestDto input, IQueryable<CustomerPackage> query)
    {
        var totalCount = await AsyncExecuter.CountAsync(query);
        query = query.Skip(input.SkipCount).Take(input.MaxResultCount);
        return (query, totalCount);
    }

    
    /// <summary>
    /// Clears the details of a customer package and sets it to an inactive state.
    /// </summary>
    /// <param name="customerPackage">The customer package to clear.</param>
    private void ClearPackageDetails(CustomerPackage customerPackage)
    {
        customerPackage.CustomerPackageStatus = CustomerPackageStatus.Deleted;
    }

    public override async Task DeleteAsync(int id)
    {
        var customerPackage = await _repository.FindAsync(id);

        if (customerPackage is null)
        {
            throw new EntityNotFoundException(typeof(CustomerPackage), id);
        }
        ClearPackageDetails(customerPackage);
        await _repository.UpdateAsync(customerPackage, true);

    }




    #endregion



}
