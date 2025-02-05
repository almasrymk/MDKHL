using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Localization;
using SmartParking.Organizations;
using SmartParking.Packages;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;


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
  
    public  async Task<CustomerPackageDto> GetOneActiveCustomerPackageByOrganizationIdAndPlateNumber(int OrganizationID,string PlateNumber)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Where(x => x.OrganizationId == OrganizationID && (x.EnPlateNumber == PlateNumber || x.ArPlateNumber==PlateNumber) && x.CustomerPackageStatus == CustomerPackageStatus.Active && x.ExpiryDate >= DateTime.Today && x.PackageId != null);
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
    public  async Task<Result> AssignPackageToCustomerAsync(int packageId, string ArabicPlateNumber, string EnglishPlateNumber)
    {
        try
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                var customer = await GetCustomerAsync();
                if (customer == null)
                    return Result.NotFound("Customer not found");

                var package = await GetPackageAsync(packageId);
                if (package == null)
                    return Result.NotFound("Package not found");

                if (await CustomerPackageExistsAsync(packageId, package.OrganizationId, customer.Id, ArabicPlateNumber, EnglishPlateNumber))
                {
                    return Result.Invalid(new ValidationError
                    {
                        ErrorCode = StatusCodes.Status400BadRequest.ToString(),
                        ErrorMessage = "This customer has the same package"
                    });
                }

                var customerPackage = CreateCustomerPackage(packageId, customer, package,ArabicPlateNumber,EnglishPlateNumber);
                var existingPackage = await GetExistingPackageAsync(package.OrganizationId);

                if (existingPackage != null)
                {
                    ObjectHelper.TrySetProperty(
                        customerPackage,
                        x => x.Id,
                        () => existingPackage.Id
                    );
                    return await UpdateCustomerPackageAsync(customerPackage);
                }

                return await InsertCustomerPackageAsync(customerPackage);
            }
        }
        catch (Exception ex)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("EmptyValidation"));
        }


    }

    private async Task<Customer> GetCustomerAsync()
    {
        return await _customerRepository.FirstOrDefaultAsync(c => c.IdentityUserId == CurrentUser.Id);
    }

    private async Task<Package> GetPackageAsync(int packageId)
    {
        return await _packageRepository.GetAsync(p => p.Id == packageId);
    }

    private async Task<bool> CustomerPackageExistsAsync(int packageId, int organizationId, Guid customerId,string ArabicPlateNumber,string EnglishPlateNumber)
    {
        var existingPackage = await _repository.FirstOrDefaultAsync(p =>
            p.PackageId == packageId &&
            p.OrganizationId == organizationId &&
            p.CustomerId == customerId &&
            p.ExpiryDate >= DateTime.Today &&
            p.CustomerPackageStatus == CustomerPackageStatus.Active &&
            p.ArPlateNumber==ArabicPlateNumber &&
            p.EnPlateNumber==EnglishPlateNumber
            );
        return existingPackage != null;
    }

    private async Task<CustomerPackage?> GetExistingPackageAsync(int organizationId)
    {
        var query = await _repository.GetQueryableAsync();
        return await query.AsNoTracking()
            .FirstOrDefaultAsync(p => p.PackageId == null && p.OrganizationId == organizationId);
    }

    private async Task<Result> UpdateCustomerPackageAsync(CustomerPackage customerPackage)
    {
        var updatedPackage = await _repository.UpdateAsync(customerPackage, true);
        return updatedPackage != null ? Result.Success() : Result.Error("Can't assign package to customer");
    }

    private async Task<Result> InsertCustomerPackageAsync(CustomerPackage customerPackage)
    {
        var assignedPackage = await _repository.InsertAsync(customerPackage, true);
        return assignedPackage != null ? Result.Success() : Result.Error("Can't assign package to customer");
    }

    private static CustomerPackage CreateCustomerPackage(int packageId, Customer? customer, Package package,string arPlateNumber,string enPlateNumber)
    {
        return  new CustomerPackage
        {
            PackageId = packageId,
            CustomerId = customer.Id,
            OrganizationId = package.OrganizationId,
            ExpiryDate = DateTime.Now.AddDays(package.DurationOfPackage).AddMinutes(60 - DateTime.Now.AddDays(package.DurationOfPackage).Minute),
            ActivationDateOfPackage = DateTime.Now,
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
            TenantId = package.TenantId,
            ArPlateNumber= arPlateNumber,
            EnPlateNumber= enPlateNumber,

        };
    }

    ///<inheritdoc/>
    public  async Task<bool> AssignPackageToCustomerWebAsync(int packageId,Guid? customerId, string ArabicPlateNumber, string EnglishPlateNumber)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var customer = await _customerRepository.FirstOrDefaultAsync(c => c.Id == customerId);
            if(customer == null)
                throw new Exception(LocalizationHelper.GetLocalizedString("Customer") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
            var package = await GetPackageAsync(packageId);

            if (package == null)
                throw new Exception(LocalizationHelper.GetLocalizedString("Package") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
            
            if (await CustomerPackageExistsAsync(packageId, package.OrganizationId, customer.Id,ArabicPlateNumber,EnglishPlateNumber))
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("Package") + " " + LocalizationHelper.GetLocalizedString("DuplicateValidation")
                    + " " + LocalizationHelper.GetLocalizedString("Customer"));
            }

            var customerPackage = CreateCustomerPackage(packageId, customer, package, ArabicPlateNumber, EnglishPlateNumber);
            var existingPackage = await GetExistingPackageAsync(package.OrganizationId);

            if (existingPackage != null)
            {
                ObjectHelper.TrySetProperty(
                    customerPackage,
                    x => x.Id,
                    () => existingPackage.Id
                );
                var updatePackage = await UpdateCustomerPackageAsync(customerPackage);
                return updatePackage.IsSuccess;
            }

            var insertPackage = await InsertCustomerPackageAsync(customerPackage);
           return insertPackage.IsSuccess;
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
        customerPackage.PackageArabicName = null;
        customerPackage.PackageEnglishName = null;
        customerPackage.PackageId = null;
        customerPackage.PublishToOfPackage = null;
        customerPackage.PublishFromOfPackage = null;
        customerPackage.PriceOfPackage = null;
        customerPackage.DurationOfPackage = null;
        customerPackage.DectivationDateOfPackage = null;
        customerPackage.DescriptionOfPackage = null;
        customerPackage.CustomerPackageStatus = CustomerPackageStatus.InActive;
        await _repository.UpdateAsync(customerPackage, true);
    }

    public async Task<PagedResultDto<CustomerPackageDto>> GetListPackagesByCustomerMdkl(Guid customerId,PagedAndSortedResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Where(c => c.CustomerId == customerId && c.PackageId != null);
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

    private async Task<(IQueryable<CustomerPackage> query, int totalCount)> ApplyPaged(PagedAndSortedResultRequestDto input, IQueryable<CustomerPackage> query)
    {
        var totalCount = await AsyncExecuter.CountAsync(query);
        query = query.Skip(input.SkipCount).Take(input.MaxResultCount);
        return (query, totalCount);
    }

    public async Task<PagedResultDto<CustomerPackageDto>> GetListPackagesByCustomerOrganization(Guid customerId, PagedAndSortedResultRequestDto input)
    {
        var org = await _organizationsApp.GetOrganizationForCurrentTenatAsync();
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Where(c => c.CustomerId == customerId && c.OrganizationId == org.Id && c.PackageId != null);
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
}
