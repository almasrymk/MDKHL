
using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartParking.Cameras;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Garages;
using SmartParking.Organizations;
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
    private readonly IOrganizationsAppService _organizationsApp;

    public CustomerPackagesAppService(IRepository<CustomerPackage, int> repository,IRepository<Package> packageRepository,IDataFilter dataFilter,IRepository<Customer, Guid> customerRepository,IOrganizationsAppService organizationsApp) :base(repository)
    {
        _repository = repository;
        _packageRepository = packageRepository;
        _dataFilter = dataFilter;
        _customerRepository = customerRepository;
        _organizationsApp = organizationsApp;
    }
  
    public  async Task<CustomerPackageDto> GetOneActiveCustomerPackageByOrgIdAndCustomerId(int orgID,Guid customerID)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Include(p => p.Customer).Include(p => p.Organization).Include(p => p.Package);
            query = query.Where(x => x.OrganizationId == orgID && x.CustomerId == customerID && x.CustomerPackageStatus == CustomerPackageStatus.Active && x.ExpiryDate >= DateTime.Today);
            var result = await query.FirstOrDefaultAsync();

            if (result == null)
            {
                return null; 
            }

            return ObjectMapper.Map<CustomerPackage, CustomerPackageDto>(result);
        }
        
    }
    ///<inheritdoc/>
    public  async Task<Result<List<PackageDto>>> GetPackageByCustomer()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _packageRepository.GetQueryableAsync();
            query = query.Where(x => x.CustomerPackages.Any(c => c.Customer.IdentityUserId == CurrentUser.Id && c.PackageId != null));

            var result = await AsyncExecuter.ToListAsync(query);

            if (result.Count == 0)
            {
                return Result<List<PackageDto>>.NotFound();
            }
            var packageMap = ObjectMapper.Map<List<Package>, List<PackageDto>>(result);
            return Result<List<PackageDto>>.Success(packageMap);
        }
    }

    ///<inheritdoc/>
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
            var customerPackageIsExist = await _repository.FirstOrDefaultAsync(p => p.PackageId == packageId && p.OrganizationId == package.OrganizationId && p.CustomerId == customer.Id && p.ExpiryDate >= DateTime.Today);
            if (customerPackageIsExist is not null)
            {
                return Result.Invalid(new ValidationError
                {
                    ErrorCode = StatusCodes.Status400BadRequest.ToString(),
                    ErrorMessage = "This customer hanve the same package"

                });
            }
            var customerPackage = new CustomerPackage
            {
                PackageId = packageId,
                CustomerId = customer.Id,
                OrganizationId = package.OrganizationId,
                ExpiryDate = DateTime.Today.AddDays(package.DurationOfPackage),
                ActivationDateOfPackage = package.ActivationDate,
                DectivationDateOfPackage = package.DectivationDate,
                DescriptionOfPackage = package.Description,
                DurationOfPackage = package.DurationOfPackage,
                PriceOfPackage = package.Price,
                PublishFromOfPackage = package.PublishFrom,
                PublishToOfPackage = package.PublishTo,
                publishedOfPackage = package.published,
                CustomerCategory = package.CustomerCategory,
                PackageArabicName = package.ArabicName,
                PackageEnglishName = package.EnglishName
                
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
            var customerPackageIsExist = await _repository.FirstOrDefaultAsync(p => p.PackageId == packageId && p.OrganizationId == package.OrganizationId && p.CustomerId == customer.Id);
            if (customerPackageIsExist is not null)
            {
                return false;
            }
            var customerPackage = new CustomerPackage
            {
                PackageId = packageId,
                CustomerId = customer.Id,
                OrganizationId = package.OrganizationId,
                ExpiryDate = DateTime.UtcNow.AddDays(package.DurationOfPackage).Date.AddDays(1).AddTicks(-1),
                ActivationDateOfPackage = package.ActivationDate,
                DectivationDateOfPackage = package.DectivationDate,
                DescriptionOfPackage = package.Description,
                DurationOfPackage = package.DurationOfPackage,
                PriceOfPackage = package.Price,
                PublishFromOfPackage = package.PublishFrom,
                PublishToOfPackage = package.PublishTo,
                publishedOfPackage = package.published,
				CustomerCategory = package.CustomerCategory,
				PackageArabicName = package.ArabicName,
				PackageEnglishName = package.EnglishName,
                
			};
            var assignPackage = await _repository.InsertAsync(customerPackage, true);
           if(assignPackage == null)
                return false;

            return true;
        }
    }

   public async Task<DateTime> GetExpireyDateForPackageSubscripedByCustomerInSpecificOrganization(int OrgId, Guid CustomerId, int PackageId)
   {

        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query =await _repository.GetQueryableAsync();
            query = query.Where(p => p.OrganizationId == OrgId && p.CustomerId == CustomerId && p.PackageId == PackageId);
            var result =await AsyncExecuter.FirstOrDefaultAsync(query);
            return result.ExpiryDate;

        }




   }
    public override async Task DeleteAsync(int id)
    {
        var customerPackage = await _repository.FindAsync(id);

        if (customerPackage is null)
        {
            throw new EntityNotFoundException(typeof(VehicleModel), id);
        }
        //customerPackage.Status = Status.Deleted;
        //await CurrentUnitOfWork.SaveChangesAsync();

        await base.DeleteAsync(id);
    }

    public async Task<List<CustomerPackageDto>> GetListPackagesByCustomerMdkl(Guid customerId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Where(c => c.CustomerId == customerId);

            var result = await AsyncExecuter.ToListAsync(query);
            var packageMap = ObjectMapper.Map<List<CustomerPackage>, List<CustomerPackageDto>>(result);
            return packageMap;
        }
    }

    public async Task<List<CustomerPackageDto>> GetListPackagesByCustomerOrganization(Guid customerId)
    {
        var org = await _organizationsApp.GetOrganizationForCurrentTenat();
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Where(c => c.CustomerId == customerId && c.OrganizationId == org.Id);

            var result = await AsyncExecuter.ToListAsync(query);
            var packageMap = ObjectMapper.Map<List<CustomerPackage>, List<CustomerPackageDto>>(result);
            return packageMap;
        }
    }
}
