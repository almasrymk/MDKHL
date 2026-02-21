using Microsoft.EntityFrameworkCore;
using SmartParking.CustomerPackages.QRGenerator;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Organizations;
using SmartParking.Packages;
using SmartParking.PDF;
using SmartParking.Results;
using SmartParking.Subscriptions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace SmartParking.CustomerPackages;

public class MobileCustomerPackagesAppService : BaseCustomerPackagesAssigingAppService, IAssignCustomerPackagesMobileAppService , ICustomerPackageValidatorAppService
{
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<CustomerPackage, int> _repository;
    private readonly ICurrentUser _currentUser;

    public MobileCustomerPackagesAppService(IRepository<CustomerPackage, int> repository, IDataFilter dataFilter,  IRepository<Customer, Guid> customerRepository, IRepository<Package, int> packageRepository, IOrganizationsAppService organizationsApp, ICustomerPackageQRGenerator customerPackageQRGenerator, ICustomerPackagePDFAppService pDFGeneratorAppService, ICurrentUser currentUser, ICustomerPackagesCreationValidationsAppService customerPackagesCreationValidations) : base(repository, dataFilter, customerRepository, packageRepository, organizationsApp, customerPackageQRGenerator, pDFGeneratorAppService, customerPackagesCreationValidations)
    {
        _dataFilter = dataFilter;
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Result<CustomerPackageDto>> AssignPackageToCustomerAsync(int packageId, string ArabicPlateNumber, string EnglishPlateNumber)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var customer = await GetCustomerByIdentityIdAsync(_currentUser.Id.Value);
            if (customer == null)
                return Result.NotFound("Customer not found");

            var package = await GetPackageAsync(packageId);
            if (package == null)
                return Result.NotFound("Package not found");
            await CheckSubscriptionCustomersLimitsAsync(package.OrganizationId);
            var customerPackage = CreateCustomerMobilePackage(package, customer.Id, ArabicPlateNumber, EnglishPlateNumber);
            var dto = CreateUpdateCustomerPackageDto.FromEntity(customerPackage);
            await HandleExistingPackagesForVehicleAsync(dto);

            customerPackage.QRCodePath = await HandleQRCode(dto, customer, customerPackage);
            CustomerPackage result = await InsertCustomerPackageAsync(customerPackage);
            return ToDto(result);

        }
    }

    #region Private Methods
    private static CustomerPackage CreateCustomerMobilePackage(PackageDto PackageDto, Guid CustomerId, string arPlateNumber, string enPlateNumber)
    {
        return new CustomerPackage
        {
            PackageId = PackageDto.Id,
            CustomerId = CustomerId,
            OrganizationId = PackageDto.OrganizationId,
            ExpiryDate = PackageDto.PackageCategoryType != PackageCategoryTypes.CustomPackage ? DateTime.Now.AddDays(PackageDto.DurationOfPackage).AddMinutes(60 - DateTime.Now.AddDays(PackageDto.DurationOfPackage).Minute) : PackageDto.DectivationDate,
            ActivationDateOfPackage = PackageDto.PackageCategoryType != PackageCategoryTypes.CustomPackage ? DateTime.Now : PackageDto.ActivationDate,
            DectivationDateOfPackage = PackageDto.PackageCategoryType != PackageCategoryTypes.CustomPackage ? DateTime.Now.AddDays(PackageDto.DurationOfPackage).AddMinutes(60 - DateTime.Now.AddDays(PackageDto.DurationOfPackage).Minute) : PackageDto.DectivationDate,
            DescriptionOfPackage = PackageDto.Description,
            DurationOfPackage = PackageDto.DurationOfPackage,
            PriceOfPackage = PackageDto.Price,
            PublishFromOfPackage = PackageDto.PublishFrom,
            PublishToOfPackage = PackageDto.PublishTo,
            publishedOfPackage = PackageDto.Published,
            CustomerCategory = Enum.TryParse<CustomerCategoryStatus>(PackageDto.CustomerCategory,out var category)? category: default,
            PackageArabicName = PackageDto.ArabicName,
            PackageEnglishName = PackageDto.EnglishName,
            TenantId = PackageDto.TenantId,
            ArPlateNumber = arPlateNumber,
            EnPlateNumber = enPlateNumber,
            Vat = Math.Round(0.15m * PackageDto.Price, 2),

        };
    }

    public async Task<bool> HandleExistingPackagesForVehicleAsync(CreateUpdateCustomerPackageDto customerPackageDto)
    {
        var newExpiryDate = customerPackageDto.ActivationDateOfPackage.Value.AddDays(-1);
        var newActivationDate = customerPackageDto.ExpiryDate.AddDays(1);

        var query = await _repository.GetQueryableAsync();

        await query
            .Where(p =>
                p.OrganizationId == customerPackageDto.OrganizationId &&
                p.EnPlateNumber == customerPackageDto.EnPlateNumber &&
                (
                    (p.ActivationDateOfPackage < customerPackageDto.ActivationDateOfPackage) || // new after current
                    (p.ActivationDateOfPackage == customerPackageDto.ActivationDateOfPackage && p.ExpiryDate == customerPackageDto.ExpiryDate) || // equal
                    (p.ActivationDateOfPackage >= customerPackageDto.ActivationDateOfPackage) // new before current
                )
            )
            .ExecuteUpdateAsync(p => p
                .SetProperty(x => x.ExpiryDate,
                    x => x.ActivationDateOfPackage < customerPackageDto.ActivationDateOfPackage
                        ? newExpiryDate
                        : x.ExpiryDate) // only update if new is after current
                .SetProperty(x => x.Status,
                    x => x.ActivationDateOfPackage == customerPackageDto.ActivationDateOfPackage &&
                         x.ExpiryDate == customerPackageDto.ExpiryDate
                        ? Status.Inactive
                        : x.Status) // only update if equal
                .SetProperty(x => x.ActivationDateOfPackage,
                    x => x.ActivationDateOfPackage >= customerPackageDto.ActivationDateOfPackage
                        ? newActivationDate
                        : x.ActivationDateOfPackage) // only update if new is before current
            );
        return true;

    }
    #endregion
}
