using Microsoft.EntityFrameworkCore;
using SmartParking.CustomerPackages.AssignantionsContarcts;
using SmartParking.CustomerPackages.Dtos;
using SmartParking.CustomerPackages.Inputs;
using SmartParking.CustomerPackages.QRGenerator;
using SmartParking.CustomerPackages.Validations;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Localization;
using SmartParking.Organizations;
using SmartParking.Packages;
using SmartParking.PDF.Contracts;
using SmartParking.Results;
using SmartParking.Subscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;

namespace SmartParking.CustomerPackages;

[RemoteService(false)]
public class NormalCustomerPackagesAppService : BaseCustomerPackagesAssigingAppService, IAssignCustomerPackagesAppService , ICustomerPackageValidatorAppService 
{
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<CustomerPackage, int> _repository;
    private readonly IObjectMapper _objectMapper;
    private readonly ICurrentTenant _currentTenant;



    public NormalCustomerPackagesAppService(
     IRepository<CustomerPackage, int> repository,
     IDataFilter dataFilter,
     IRepository<Customer, Guid> customerRepository,
     IRepository<Package, int> packageRepository,
     IOrganizationsAppService organizationsAppService,
     ICustomerPackageQRGenerator qrGenerator,
     ICustomerPackagePDFAppService pdfAppService,
     ICustomerPackagesCreationValidationsAppService customerPackagesCreationValidations
,
     IObjectMapper objectMapper,
     ICurrentTenant currentTenant) : base(repository, dataFilter, customerRepository, packageRepository, organizationsAppService, qrGenerator, pdfAppService, customerPackagesCreationValidations, objectMapper ,currentTenant)
    {
        _dataFilter = dataFilter;
        _repository = repository;
        _objectMapper = objectMapper;
        _currentTenant = currentTenant;
    }

    public async Task<Result<CustomerPackageDto>> AssignPackageToCustomerAsync(CreateUpdateCustomerPackageDto createUpdateCustomerPackageDto)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var customer = await GetCustomerByCustomerIdAsync(createUpdateCustomerPackageDto.CustomerId);
            await CheckSubscriptionCustomersLimitsAsync(createUpdateCustomerPackageDto.OrganizationId);
            CustomerPackage customerPackage;
            await HandleExistingPackagesForVehicleAsync(createUpdateCustomerPackageDto);
            if (createUpdateCustomerPackageDto.PackageId != 0 && createUpdateCustomerPackageDto.PackageId != null)
            {
                var package = await GetPackageAsync((int)createUpdateCustomerPackageDto!.PackageId);
                if (package == null)
                    throw new Exception(LocalizationHelper.GetLocalizedString("Package") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
                customerPackage = CreateCustomerPackage(createUpdateCustomerPackageDto, package);
            }
            else
            {
                customerPackage =createUpdateCustomerPackageDto.ToEntity();
                customerPackage.PackageId = null;
                customerPackage.Package = null;
            }
            customerPackage.QRCodePath = await HandleQRCode(createUpdateCustomerPackageDto, customer, customerPackage);
            var insertPackage = await InsertCustomerPackageAsync(customerPackage);
            return ToDto(insertPackage);
        }
    }


    public async Task<bool> HandleExistingPackagesForVehicleAsync(CreateUpdateCustomerPackageDto customerPackageDto)
    {
        return await base.HandleExistingPackagesForVehicleAsync(customerPackageDto);
    }

    public async Task<CustomerPackageDto> UpdateCustomerPackage(int customerPackageId, CreateUpdateCustomerPackageDto input)
    {
        return await base.UpdateCustomerPackage(customerPackageId, input);
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
    private CustomerPackage CreateCustomerPackage(CreateUpdateCustomerPackageDto customerPackageDto, PackageDto package)
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
