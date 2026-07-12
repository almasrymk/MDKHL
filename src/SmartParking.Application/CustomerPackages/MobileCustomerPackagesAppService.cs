using Microsoft.EntityFrameworkCore;
using SmartParking.CustomerPackages.AssignantionsContarcts;
using SmartParking.CustomerPackages.Dtos;
using SmartParking.CustomerPackages.Inputs;
using SmartParking.CustomerPackages.QRGenerator;
using SmartParking.CustomerPackages.Validations;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Organizations;
using SmartParking.Packages;
using SmartParking.PDF.Contracts;
using SmartParking.Results;
using SmartParking.Subscriptions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;

namespace SmartParking.CustomerPackages;
[RemoteService(false)]

public class MobileCustomerPackagesAppService : BaseCustomerPackagesAssigingAppService, IAssignCustomerPackagesMobileAppService , ICustomerPackageValidatorAppService
{
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<CustomerPackage, int> _repository;
    private readonly ICurrentUser _currentUser;
    private readonly IObjectMapper _objectMapper;
    private readonly ICurrentTenant _currentTenant;

    public MobileCustomerPackagesAppService(IRepository<CustomerPackage, int> repository, IDataFilter dataFilter, IRepository<Customer, Guid> customerRepository, IRepository<Package, int> packageRepository, IOrganizationsAppService organizationsApp, ICustomerPackageQRGenerator customerPackageQRGenerator, ICustomerPackagePDFAppService pDFGeneratorAppService, ICurrentUser currentUser, ICustomerPackagesCreationValidationsAppService customerPackagesCreationValidations, IObjectMapper objectMapper, ICurrentTenant currentTenant) : base(repository, dataFilter, customerRepository, packageRepository, organizationsApp, customerPackageQRGenerator, pDFGeneratorAppService, customerPackagesCreationValidations, objectMapper , currentTenant)
    {
        _dataFilter = dataFilter;
        _repository = repository;
        _currentUser = currentUser;
        _objectMapper = objectMapper;
        _currentTenant = currentTenant;
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
            customerPackage.TenantId = package.TenantId;

            customerPackage.QRCodePath = await HandleQRCode(dto, customer, customerPackage);
            var defaultPackage = await UpdateDefaultCustomerPackage(dto);
            if (defaultPackage.IsSuccess)
                return defaultPackage;
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
        return await base.HandleExistingPackagesForVehicleAsync(customerPackageDto);
    }
    #endregion
}
