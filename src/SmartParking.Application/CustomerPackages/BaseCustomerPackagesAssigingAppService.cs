using Microsoft.EntityFrameworkCore;
using SmartParking.CustomerPackages.QRGenerator;
using SmartParking.Cutomers;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Localization;
using SmartParking.Organizations;
using SmartParking.Packages;
using SmartParking.PDF;
using SmartParking.Results;
using SmartParking.Subscriptions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.CustomerPackages;
[RemoteService(false)]

public abstract class BaseCustomerPackagesAssigingAppService : SmartParkingAppService, ICustomerPackagesAssigningAppService , IApplicationService
{
    private readonly IRepository<CustomerPackage, int> _repository;
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Package, int> _packageRepository;
    private readonly IOrganizationsAppService _organizationsApp;
    private readonly ICustomerPackageQRGenerator _customerPackageQRGenerator;
    private readonly ICustomerPackagePDFAppService _pdfGeneratorAppService;
    private readonly ICustomerPackagesCreationValidationsAppService _customerPackageCreationValidation;

    public BaseCustomerPackagesAssigingAppService(IRepository<CustomerPackage, int> repository, IDataFilter dataFilter, IRepository<Customer, Guid> customerRepository, IRepository<Package, int> packageRepository, IOrganizationsAppService organizationsApp, ICustomerPackageQRGenerator customerPackageQRGenerator, ICustomerPackagePDFAppService pDFGeneratorAppService, ICustomerPackagesCreationValidationsAppService customerPackageCreationValidation)
    {
        _repository = repository;
        _dataFilter = dataFilter;
        _customerRepository = customerRepository;
        _packageRepository = packageRepository;
        _organizationsApp = organizationsApp;
        _customerPackageQRGenerator = customerPackageQRGenerator;
        _pdfGeneratorAppService = pDFGeneratorAppService;
        _customerPackageCreationValidation = customerPackageCreationValidation;
    }




    /// <summary>
    /// Retrieves a package entity by its ID.
    /// </summary>
    /// <param name="packageId">The ID of the package to retrieve.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the <see cref="Package"/> entity if found; otherwise, null.
    /// </returns>
    public virtual async Task<PackageDto> GetPackageAsync(int packageId)
    {
        var package= await _packageRepository.FirstOrDefaultAsync(p => p.Id == packageId);
        return package != null ? new PackageDto
        {
            Id = package.Id,
            ArabicName = package.ArabicName,
            EnglishName = package.EnglishName,
            Price = package.Price,
            Description = package.Description,
            Published = package.published,
            PublishFrom = package.PublishFrom,
            PublishTo = package.PublishTo,
            CreatedDate = package.CreatedDate,
            CreatedBy = package.CreatedBy,
            ActivationDate = package.ActivationDate,
            ActivedBy = package.ActivedBy,
            DectivationDate = package.DectivationDate,
            DeactivatedBy = package.DeactivatedBy,
            OrganizationId = package.OrganizationId,
            OrganizationArabicName = package.Organization != null ? package.Organization.ArabicName : string.Empty,
            OrganizationEnglishName = package.Organization != null ? package.Organization.EnglishName : string.Empty,
            PackageStatus = package.PackageStatus,
            CustomerCategory=package.CustomerCategory.ToString(),
            DurationOfPackage=package.DurationOfPackage,
            Status=package.Status,
            PackageCategoryType=package.PackageCategoryType
        } : null;

    }
    public async Task CheckSubscriptionCustomersLimitsAsync(int OrganizationId)
    {
        await _customerPackageCreationValidation.CheckSubscriptionCustomersLimitsAsync(OrganizationId);
    }
    public async Task<string> HandleQRCode(CreateUpdateCustomerPackageDto customerPackageDto, CustomerDto customer, CustomerPackage customerPackage)
    {
        customerPackage.PackageCode = await CreatePackageCode(customerPackageDto);
        if (customerPackage.PackageCode != null)
        {
            var qrCodeDto = new CreateUpdateQrCodeDto { PhoneNumber = customer.MobileNumber, OrganizationId = (int)customerPackage.OrganizationId!, PackageCode = customerPackage.PackageCode };
            var qrCodeImage = await _customerPackageQRGenerator.CreateAsync(qrCodeDto);
            string pdfPath = await _pdfGeneratorAppService.GeneratePdfForCustomerPackage(qrCodeDto, qrCodeImage);
            return pdfPath;
        }
        return "";
    }
    /// <summary>
    /// Inserts a new customer package into the database.
    /// </summary>
    /// <param name="customerPackage">The customer package to insert.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="Result"/> indicating success or failure.
    /// </returns>
    protected async Task<Result<CustomerPackage>> InsertCustomerPackageAsync(CustomerPackage customerPackage)
    {
        customerPackage.TenantId = CurrentTenant.Id;
        var assignedPackage = await _repository.InsertAsync(customerPackage, true);
        return assignedPackage != null ? assignedPackage : Result.Error("Can't assign package to customer");
    }

    public virtual async Task<CustomerDto> GetCustomerByCustomerIdAsync(Guid customerId)
    {
        var customer = await _customerRepository
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer == null)
            throw new Exception(
                LocalizationHelper.GetLocalizedString("Customer") + " " +
                LocalizationHelper.GetLocalizedString("NotFoundValidation"));

        return new CustomerDto
        {
            ArabicName = customer.ArabicName,
            Email = customer.IdentityUser.Email,
            FullName = customer.IdentityUser.Name,
            MobileNumber = customer.IdentityUser.PhoneNumber,
            Id = customer.Id,
        };
    }

    public virtual async Task<CustomerDto> GetCustomerByIdentityIdAsync(Guid identityId)
    {
        var customer = await _customerRepository
            .FirstOrDefaultAsync(c => c.IdentityUser.Id == identityId);

        if (customer == null)
            throw new Exception(
                LocalizationHelper.GetLocalizedString("Customer") + " " +
                LocalizationHelper.GetLocalizedString("NotFoundValidation"));

        return new CustomerDto
        {
            ArabicName = customer.ArabicName,
            Email = customer.IdentityUser.Email,
            FullName = customer.IdentityUser.Name,
            MobileNumber = customer.IdentityUser.PhoneNumber,
            Id = customer.Id,
        };
    }
    public CustomerPackageDto ToDto(CustomerPackage entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        // Convert DateTime -> DateOnly/TimeOnly for DTO fields not موجودة in entity
        var expiryDateOnly = DateOnly.FromDateTime(entity.ExpiryDate);
        var expiryTimeOnly = TimeOnly.FromDateTime(entity.ExpiryDate);

        DateOnly? activationDate = null;
        TimeOnly? activationTime = null;

        if (entity.ActivationDateOfPackage.HasValue)
        {
            activationDate = DateOnly.FromDateTime(entity.ActivationDateOfPackage.Value);
            activationTime = TimeOnly.FromDateTime(entity.ActivationDateOfPackage.Value);
        }

        return new CustomerPackageDto
        {
            Id = entity.Id,

            CustomerId = entity.CustomerId,

            // DTO has int (non-nullable) but entity has int?
            PackageId = entity.PackageId ?? 0,
            OrganizationId = entity.OrganizationId ?? 0,

            CustomerPackageStatus = entity.CustomerPackageStatus,
            TenantId = entity.TenantId,

            DurationOfPackage = entity.DurationOfPackage,
            PriceOfPackage = entity.PriceOfPackage,
            PriceOfPackagePerDay = entity.PriceOfPackagePerDay,
            CountOfDays = entity.CountOfDays,
            DescriptionOfPackage = entity.DescriptionOfPackage,
            publishedOfPackage = entity.publishedOfPackage,

            PublishFromOfPackage = entity.PublishFromOfPackage,
            PublishToOfPackage = entity.PublishToOfPackage,

            ActivationDateOfPackage = entity.ActivationDateOfPackage,
            DectivationDateOfPackage = entity.DectivationDateOfPackage,

            ExpiryDate = entity.ExpiryDate,

            PackageArabicName = entity.PackageArabicName ?? string.Empty,
            PackageEnglishName = entity.PackageEnglishName ?? string.Empty,

            CustomerCategory = entity.CustomerCategory,

            EnPlateNumber = entity.EnPlateNumber ?? string.Empty,
            ArPlateNumber = entity.ArPlateNumber ?? string.Empty,

            // derived from ExpiryDate
            ExpiryDateOfPackage = expiryDateOnly,
            ExpiryTimeOfPackage = expiryTimeOnly,

            // derived from ActivationDateOfPackage (nullable)
            ActivationDate = activationDate,
            ActivationTime = activationTime,

            IsCustomPackage = entity.IsCustomPackage,
            PackageCode = entity.PackageCode,
            QRCodePath = entity.QRCodePath,

            Discount = entity.Discount,
            Vat = entity.Vat,

            // ====== Values موجودة في DTO فقط (not in entity) ======
            LicenseTypeImage = null,
            VehicleModelImage = null,

            // ====== Navigation-based fields (safe even if not loaded) ======
            CustomerArabicName = entity.Customer?.ArabicName ?? string.Empty,
            CustomerEnglishName = entity.Customer?.IdentityUser.Name ?? string.Empty,

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
                    p.ActivationDateOfPackage < customerPackageDto.ActivationDateOfPackage ||
                    p.ActivationDateOfPackage == customerPackageDto.ActivationDateOfPackage ||
                    p.ActivationDateOfPackage > customerPackageDto.ActivationDateOfPackage
                )
            )
            .ExecuteUpdateAsync(p => p

        // 1. Update Status to Inactive ONLY if the Activation Dates match
        .SetProperty(x => x.Status,
            x => x.ActivationDateOfPackage == customerPackageDto.ActivationDateOfPackage
                ? Status.Inactive
                : x.Status)

        // 2. Update ExpiryDate if the existing package is strictly BEFORE the new one
        .SetProperty(x => x.ExpiryDate,
            x => x.ActivationDateOfPackage < customerPackageDto.ActivationDateOfPackage
                ? newExpiryDate
                : x.ExpiryDate)

        // 3. Update ActivationDate if the existing package is strictly AFTER the new one
        .SetProperty(x => x.ActivationDateOfPackage,
            x => x.ActivationDateOfPackage > customerPackageDto.ActivationDateOfPackage
                ? (newActivationDate < x.ExpiryDate ? newActivationDate : x.ExpiryDate)
                : x.ActivationDateOfPackage)
    );

        return true;
    }



    #region Private Methods


    private async Task<string> CreatePackageCode(CreateUpdateCustomerPackageDto customerPackageDto)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var org = await _organizationsApp.GetAsync(customerPackageDto.OrganizationId);
            if (!org.DispenserTransaction)
                return null;
            string code;
            do
            {
                code = GeneratePackageCode(org.PaymentCode, org.EnglishName);
            }
            while (await IsCodeUnique(code));
            return code;
        }
    }
    private async Task<bool> IsCodeUnique(string Code)
    {
        return await _repository.AnyAsync(cp => cp.PackageCode == Code);
    }


    private static string GeneratePackageCode(string PayemntCode, string EnglishName)
    {
        var random = new Random();
        int customerCount = random.Next(1, 10000);
        var idLen = (int)Math.Floor(Math.Log10(customerCount) + 1);
        int zeros = (9 - idLen);
        string Code = PayemntCode ?? EnglishName.Substring(0, 3);
        Code = Code.PadRight(zeros + 1, '0');
        Code += customerCount;
        return Code;
    }
    #endregion
}
