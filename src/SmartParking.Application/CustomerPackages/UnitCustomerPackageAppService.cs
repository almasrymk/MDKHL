using Microsoft.EntityFrameworkCore;
using SmartParking.CustomerPackages.AssignantionsContarcts;
using SmartParking.CustomerPackages.Dtos;
using SmartParking.CustomerPackages.Inputs;
using SmartParking.CustomerPackages.QRGenerator;
using SmartParking.CustomerPackages.Validations;
using SmartParking.CustomerUnitPackages;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Localization;
using SmartParking.Organizations;
using SmartParking.Packages;
using SmartParking.PDF;
using SmartParking.Results;
using SmartParking.UnitPackages;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.CustomerPackages;

public class UnitCustomerPackageAppService : BaseCustomerPackagesAssigingAppService, IAssignUnitCustomerPackageAppService, ICustomerPackageValidatorAppService
{
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<CustomerPackage, int> _repository;
    private readonly IRepository<UnitPackage, int> _unitPackageRepsoitory;


    public UnitCustomerPackageAppService(IRepository<CustomerPackage, int> repository, IDataFilter dataFilter, IRepository<Customer, Guid> customerRepository, IRepository<Package, int> packageRepository, IOrganizationsAppService organizationsApp, ICustomerPackageQRGenerator customerPackageQRGenerator, ICustomerPackagePDFAppService pDFGeneratorAppService, ICustomerPackagesCreationValidationsAppService customerPackageCreationValidation, IRepository<UnitPackage, int> unitPackageRepsoitory) : base(repository, dataFilter, customerRepository, packageRepository, organizationsApp, customerPackageQRGenerator, pDFGeneratorAppService, customerPackageCreationValidation)
    {
        _dataFilter = dataFilter;
        _repository = repository;
        _unitPackageRepsoitory = unitPackageRepsoitory;
    }

    public async Task<Result<CustomerPackageDto>> AssignPackageToCustomerAsync(CreateUpdateCustomerPackageDto createUpdateCustomerPackageDto)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var customer = await GetCustomerByCustomerIdAsync(createUpdateCustomerPackageDto.CustomerId);
            await CheckSubscriptionCustomersLimitsAsync(createUpdateCustomerPackageDto.OrganizationId);
            CustomerPackage customerPackage;
            if (createUpdateCustomerPackageDto.PackageId != 0 && createUpdateCustomerPackageDto.PackageId != null)
            {
                var package = await GetUnitPackageAsync((int)createUpdateCustomerPackageDto!.PackageId);
                if (package == null)
                    throw new Exception(LocalizationHelper.GetLocalizedString("Package") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
                customerPackage = CreateCustomerPackage(createUpdateCustomerPackageDto,package.Package);
            }
            else
            {
                customerPackage = createUpdateCustomerPackageDto.ToEntity();
                customerPackage.PackageId = null;
                customerPackage.Package = null;
            }
            customerPackage.QRCodePath = await HandleQRCode(createUpdateCustomerPackageDto, customer, customerPackage);
            var insertPackage = await InsertCustomerPackageAsync(customerPackage);
            return ToDto(insertPackage);
        }
    }

    public async Task<UnitPackageDto> GetUnitPackageAsync(int packageId)
    {
        var result= await _unitPackageRepsoitory.GetAsync(packageId);
        return ObjectMapper.Map<UnitPackage, UnitPackageDto>(result);

    }

    public async Task<bool> HandleExistingPackagesForVehicleAsync(CreateUpdateCustomerPackageDto customerPackageDto)
    {
        return await base.HandleExistingPackagesForVehicleAsync(customerPackageDto);
    }

    #region Private Methods
    /// <summary>
    /// Creates a new <see cref="CustomerPackage"/> instance from the provided package and customer information.
    /// </summary>
    /// <param name="packageId">The ID of the package.</param>
    /// <param name="customerId">The ID of the customer.</param>
    /// <param name="package">The package entity.</param>
    /// <param name="arPlateNumber">The Arabic plate number.</param>
    /// <param name="enPlateNumber">The English plate number.</param>
    /// <returns>
    /// A new <see cref="CustomerPackage"/> object populated with package and customer data.
    /// </returns>
    private CustomerPackage CreateCustomerPackage(CreateUpdateCustomerPackageDto customerPackageDto , PackageDto package)
    {
        return new CustomerPackage
        {
            PackageId = customerPackageDto.PackageId,
            CustomerId = customerPackageDto.CustomerId,
            OrganizationId = customerPackageDto.OrganizationId,
            ExpiryDate = package.PackageCategoryType != PackageCategoryTypes.CustomPackage ? DateTime.Now.AddDays(package.DurationOfPackage).AddMinutes(60 - DateTime.Now.AddDays(package.DurationOfPackage).Minute) : package.DectivationDate,
            ActivationDateOfPackage = package.PackageCategoryType != PackageCategoryTypes.CustomPackage ? DateTime.Now : package.ActivationDate,
            DectivationDateOfPackage = package.PackageCategoryType != PackageCategoryTypes.CustomPackage ? DateTime.Now.AddDays(package.DurationOfPackage).AddMinutes(60 - DateTime.Now.AddDays(package.DurationOfPackage).Minute) : package.DectivationDate,
            DescriptionOfPackage = customerPackageDto.DescriptionOfPackage,
            DurationOfPackage = customerPackageDto.DurationOfPackage,
            PriceOfPackage = customerPackageDto.PriceOfPackage,
            PublishFromOfPackage = customerPackageDto.PublishFromOfPackage,
            PublishToOfPackage = customerPackageDto.PublishToOfPackage,
            publishedOfPackage = customerPackageDto.PublishedOfPackage,
            CustomerCategory = customerPackageDto.CustomerCategory,
            PackageArabicName = customerPackageDto.PackageArabicName,
            PackageEnglishName = customerPackageDto.PackageEnglishName,
            TenantId = customerPackageDto.TenantId,
            ArPlateNumber = customerPackageDto.ArPlateNumber,
            EnPlateNumber = customerPackageDto.EnPlateNumber,
            Vat = customerPackageDto.Vat,
            Discount = customerPackageDto.Discount,
            
        };
    }


    #endregion
}
