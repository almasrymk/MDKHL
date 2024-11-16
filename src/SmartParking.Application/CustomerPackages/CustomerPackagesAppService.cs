
using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Authorization;
using SmartParking.Cameras;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Garages;
using SmartParking.Packages;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;

namespace SmartParking.CustomerPackages;
//[Authorize]
public class CustomerPackagesAppService:
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
    private readonly IRepository<Package> _packageRepository;
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<Customer, Guid> _customerRepository;

    public CustomerPackagesAppService(IRepository<CustomerPackage, int> repository,IRepository<Package> packageRepository,IDataFilter dataFilter,IRepository<Customer, Guid> customerRepository) :base(repository)
    {
        _repository = repository;
        _packageRepository = packageRepository;
        _dataFilter = dataFilter;
        _customerRepository = customerRepository;
    }
  
    public  async Task<CustomerPackageDto> GetOneActiveCustomerPackageByOrgIdAndCustomerId(int orgID,Guid customerID)
    {
        var result=await _repository.FirstOrDefaultAsync(x=>x.Package.OrganizationId == orgID&&x.CustomerId==customerID&&x.CustomerPackageStatus==CustomerPackageStatus.Active);
        
        if (result == null)
        {
            return null; // Or throw an appropriate exception if necessary
        }
       
        return  ObjectMapper.Map<CustomerPackage,CustomerPackageDto>(result);
    }
    public  async Task<Result<List<PackageDto>>> GetPackageByCustomer()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _packageRepository.GetQueryableAsync();
            query = query.Where(x => x.CustomerPackages.Any(c => c.Customer.IdentityUserId == CurrentUser.Id));

            var result = await AsyncExecuter.ToListAsync(query);

            if (result.Count == 0)
            {
                return Result<List<PackageDto>>.NotFound();
            }
            var packageMap = ObjectMapper.Map<List<Package>, List<PackageDto>>(result);
            return Result<List<PackageDto>>.Success(packageMap);
        }
    }

    [RemoteService(IsEnabled =false)]
    public  async Task<Result> AssignPackageToCustomerAsync(int packageId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var customer = await _customerRepository.FirstOrDefaultAsync(c => c.IdentityUserId == CurrentUser.Id);
            if(customer == null)
                return Result.NotFound("Customer not found");
            var package = await _packageRepository.GetAsync(p => p.Id == packageId);

            if (package == null)
                return Result.NotFound("Package not found");
            var customerPackage = new CustomerPackage
            {
                PackageId = packageId,
                CustomerId = customer.Id,
                OrganizationId = package.OrganizationId
            };
            var assignPackage = await _repository.InsertAsync(customerPackage, true);
           if(assignPackage == null)
                return Result.Error("Can't assign package to customer");

            return Result.Success();
        }
    }
   ///<inheritdoc/>
    public  async Task<bool> AssignPackageToCustomerWebAsync(int packageId,Guid customerId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var customer = await _customerRepository.FirstOrDefaultAsync(c => c.Id == customerId);
            if(customer == null)
                return false;
            var package = await _packageRepository.GetAsync(p => p.Id == packageId);

            if (package == null)
                return false;
            var customerPackage = new CustomerPackage
            {
                PackageId = packageId,
                CustomerId = customer.Id,
                OrganizationId = package.OrganizationId
            };
            var assignPackage = await _repository.InsertAsync(customerPackage, true);
           if(assignPackage == null)
                return false;

            return true;
        }
    }

    public override async Task DeleteAsync(int id)
    {
        var customerPackage = await _repository.FindAsync(id);

        if (customerPackage is null)
        {
            throw new EntityNotFoundException(typeof(VehicleModel), id);
        }
        customerPackage.Status = Status.Deleted;
        await CurrentUnitOfWork.SaveChangesAsync();

        //await base.DeleteAsync(id);
    }
}
