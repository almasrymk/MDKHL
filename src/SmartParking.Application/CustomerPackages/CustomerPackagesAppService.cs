using Castle.Core.Resource;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using SmartParking.CustomerPackages.QRGenerator;
using SmartParking.Cutomers;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Subscriptions;
using SmartParking.Enums;
using SmartParking.Localization;
using SmartParking.Organizations;
using SmartParking.Packages;
using SmartParking.Payments;
using SmartParking.PDF;
using SmartParking.Results;
using SmartParking.Utilities;
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
using Volo.Abp.ObjectMapping;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;


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
    private readonly IRepository<Package> _packageRepository;
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IOrganizationsAppService _organizationsApp;
    private readonly IRepository<Subscription, int> _subscriptionRepository;
    private readonly IRepository<Organization, int> _organizationsRepository;
    private readonly ICustomerPackageQRGenerator _customerPackageQRGenerator;
    private readonly ICustomerPackagePDFAppService _pDFGeneratorAppService;

    public CustomerPackagesAppService(IRepository<CustomerPackage, int> repository, IRepository<Package> packageRepository, IDataFilter dataFilter, IRepository<Customer, Guid> customerRepository, IOrganizationsAppService organizationsApp, IRepository<Subscription, int> subscriptionRepository, IRepository<Organization, int> organizationsRepository, ICustomerPackageQRGenerator customerPackageQRGenerator, ICustomerPackagePDFAppService pDFGeneratorAppService) : base(repository)
    {
        _repository = repository;
        _packageRepository = packageRepository;
        _dataFilter = dataFilter;
        _customerRepository = customerRepository;
        _organizationsApp = organizationsApp;
        _subscriptionRepository = subscriptionRepository;
        _organizationsRepository = organizationsRepository;
        _customerPackageQRGenerator = customerPackageQRGenerator;
        _pDFGeneratorAppService = pDFGeneratorAppService;
    }
    #region public methods

    ///<inheritdoc/>
    public async Task<Result<List<CustomerPackageDto>>> GetPackageByCustomer()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Where(c => c.Customer.IdentityUserId == CurrentUser.Id && c.PackageId != null);

            var result = await AsyncExecuter.ToListAsync(query);

            var packageMap = ObjectMapper.Map<List<CustomerPackage>, List<CustomerPackageDto>>(result);
            return Result<List<CustomerPackageDto>>.Success(packageMap);
        }
    }

    ///<inheritdoc/>
    [RemoteService(IsEnabled = false)]
    public async Task<Result<PaymentCheckoutRequestDto>> AssignPackageToCustomerAsync(int packageId, string ArabicPlateNumber, string EnglishPlateNumber)
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


            var customerPackage = CreateCustomerMobilePackage(package, customer.Id, ArabicPlateNumber, EnglishPlateNumber);
            var existingPackage = await GetExistingPackageAsync(package.OrganizationId);
            customerPackage.QRCodePath = await HandleQRCode(new CustomerPackageDto { OrganizationId = package.OrganizationId, PackageId = packageId, ArPlateNumber = ArabicPlateNumber, EnPlateNumber = EnglishPlateNumber }, customer, customerPackage);
            CustomerPackage result = null;
            if (existingPackage != null)
            {
                ObjectHelper.TrySetProperty(
                    customerPackage,
                    x => x.Id,
                    () => existingPackage.Id
                );
                result = await UpdateCustomerPackageAsync(customerPackage);
            }
            if(result == null)
                result = await InsertCustomerPackageAsync(customerPackage);
            var packageDto = ObjectMapper.Map<CustomerPackage, CustomerPackageDto>(result);
            var paymentRqDto = RequestToPaymentCheckout(packageDto, package.TenantId.Value);
            return Result.Success(paymentRqDto);

        }

    }
    ///<inheritdoc/>
    public async Task<int> AssignPackageToCustomerWebAsync(int? packageId, CustomerPackageDto customerPackageDto)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var customer = await _customerRepository.FirstOrDefaultAsync(c => c.Id == customerPackageDto.CustomerId);
            if (customer == null)
                throw new Exception(LocalizationHelper.GetLocalizedString("Customer") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
            if (await CustomerPackageExistsAsync(packageId, customerPackageDto.OrganizationId, customer.Id, customerPackageDto.ArPlateNumber, customerPackageDto.EnPlateNumber))
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("Package") + " " + LocalizationHelper.GetLocalizedString("DuplicateValidation")
                    + " " + LocalizationHelper.GetLocalizedString("Customer"));
            }
            await CheckPackageDuplication(customerPackageDto);
            await CheckSubscriptionCustomersLimitsAsync(customerPackageDto.OrganizationId);
            CustomerPackage customerPackage;

            if (packageId != 0)
            {
                var result = await AssignNormalPackageToCUstomerAsync(customerPackageDto);
                customerPackage = result.Item1;
                if (result.Item2)
                {
                    customerPackage.QRCodePath = await HandleQRCode(customerPackageDto, customer, customerPackage);
                    await CurrentUnitOfWork.SaveChangesAsync();
                    return customerPackage.Id;
                }
            }
            else
            {
                customerPackage = ObjectMapper.Map<CustomerPackageDto, CustomerPackage>(customerPackageDto);
                customerPackage.PackageId = null;
                customerPackage.Package = null;
            }
            customerPackage.QRCodePath = await HandleQRCode(customerPackageDto, customer, customerPackage);
            var insertPackage = await InsertCustomerPackageAsync(customerPackage);
            return insertPackage.Value.Id;
        }
    }

    private async Task<string> HandleQRCode(CustomerPackageDto customerPackageDto, Customer customer, CustomerPackage customerPackage)
    {
        customerPackage.PackageCode = await CreatePackageCode(customerPackageDto);
        if (customerPackage.PackageCode != null)
        {
            var qrCodeDto = new CreateUpdateQrCodeDto { PhoneNumber = customer.IdentityUser.PhoneNumber, OrganizationId = (int)customerPackage.OrganizationId!, PackageCode = customerPackage.PackageCode };
            var qrCodeImage = await _customerPackageQRGenerator.CreateAsync(qrCodeDto);
            string pdfPath = await _pDFGeneratorAppService.GeneratePdfForCustomerPackage(qrCodeDto, qrCodeImage);
            return pdfPath;
        }
        return "";
    }

    private async Task<string> CreatePackageCode(CustomerPackageDto customerPackageDto)
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
    private async Task<CustomerPackageTransactionDto> GetOneActiveCustomerPackageByOrganizationIdAndPlateNumberForGarageTransaction(int OrganizationID, string PlateNumber, DateTime ExitTransaction)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Where(x => x.OrganizationId == OrganizationID && (x.EnPlateNumber == PlateNumber || x.ArPlateNumber == PlateNumber) && x.CustomerPackageStatus == CustomerPackageStatus.Active && x.ExpiryDate >= ExitTransaction && ((x.IsCustomPackage && x.PackageId == null) || (!x.IsCustomPackage && x.PackageId != null)));
            var customerPackageDto = await AsyncExecuter.FirstOrDefaultAsync(query);
            return ObjectMapper.Map<CustomerPackage, CustomerPackageTransactionDto>(customerPackageDto);
        }

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

    private async Task CheckPackageDuplication(CustomerPackageDto customerPackageDto)
    {
        var activePackage = await GetOneActiveCustomerPackageByOrganizationIdAndPlateNumberForGarageTransaction(customerPackageDto.OrganizationId, customerPackageDto.EnPlateNumber, DateTime.Now);
        if (activePackage != null)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("CannotAssignTwoPackageToSameVehicle"));
        }
    }


    ///<inheritdoc/>

    public async Task CheckSubscriptionCustomersLimitsAsync(int organizationId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var organization = await _organizationsRepository.GetAsync(organizationId);
            int numberOfCustomers = await GetNumberOfCustomerAtOrganizationAsync(organizationId);
            Subscription? subscription = await GetSubscriptionByOrganizationId(organization);

            if (subscription == null || subscription.NumberOfCustomers < numberOfCustomers + 1)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("UpgradeSubscriptionValidationUser"));
            }

        }
    }

    ///<inheritdoc/>

    public async Task<DateTime> GetExpireyDateForPackageSubscripedByCustomerInSpecificOrganization(int OrgId, Guid CustomerId, int PackageId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Where(p => p.OrganizationId == OrgId && p.CustomerId == CustomerId && p.PackageId == PackageId);
            var result = await AsyncExecuter.FirstOrDefaultAsync(query);
            return result.ExpiryDate;
        }
    }

    ///<inheritdoc/>

    public async Task<PagedResultDto<CustomerPackageDto>> GetListPackagesByCustomerMdkl(Guid customerId, PagedAndSortedResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Where(c => c.CustomerId == customerId && (c.PackageId.HasValue || c.IsCustomPackage));
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
            query = query.Where(c => c.CustomerId == customerId && c.OrganizationId == org.Id && (c.PackageId.HasValue || c.IsCustomPackage));
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
    public async Task AddDefaultPackageToCustomerAsync(Guid CustomerId)
    {
        var org = await _organizationsApp.GetOrganizationForCurrentTenatAsync();
        await InsertCustomerPackageAsync(new CustomerPackage
        {
            CustomerId = CustomerId,
            OrganizationId = org.Id,
            TenantId = org.TenantId,
        });

    }

    public override async Task<CustomerPackageDto> GetAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await base.GetAsync(id);

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
    /// Retrieves the number of customers associated with a specific organization.
    /// </summary>
    /// <param name="organizationId">The ID of the organization.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains the number of customers in the organization.
    /// </returns>
    private async Task<int> GetNumberOfCustomerAtOrganizationAsync(int organizationId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _customerRepository.GetQueryableAsync();
            query = query.Include(x => x.CustomerPackages)
                         .Where(x => x.CustomerPackages != null && x.CustomerPackages.Any(o => o.OrganizationId == organizationId));

            var count = await AsyncExecuter.CountAsync(query);
            return count;
        }
    }
    /// <summary>
    /// Assigns a normal package to a customer. If the customer already has a package in the same organization, it updates the existing one.
    /// </summary>
    /// <param name="packageId">The ID of the package to assign.</param>
    /// <param name="customerId">The ID of the customer.</param>
    /// <param name="ArabicPlateNumber">The Arabic plate number of the customer’s vehicle.</param>
    /// <param name="EnglishPlateNumber">The English plate number of the customer’s vehicle.</param>
    /// <returns>
    /// A tuple containing the <see cref="CustomerPackage"/> and a boolean flag indicating whether the package was updated (true) or newly assigned (false).
    /// </returns>
    private async Task<(CustomerPackage, bool)> AssignNormalPackageToCUstomerAsync(CustomerPackageDto customerPackageDto)
    {
        var package = await GetPackageAsync(customerPackageDto!.PackageId);
        if (package == null)
            throw new Exception(LocalizationHelper.GetLocalizedString("Package") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
        var customerPackage = CreateCustomerPackage(customerPackageDto);
        var existingPackage = await GetExistingPackageAsync(package.OrganizationId);
        bool flag = false;
        if (existingPackage != null)
        {
            ObjectHelper.TrySetProperty(
                customerPackage,
                x => x.Id,
                () => existingPackage.Id
            );
            // update 
            await UpdateCustomerPackageAsync(customerPackage);
            flag = true;

        }
        return (customerPackage, flag);
    }
    /// <summary>
    /// Clears the details of a customer package and sets it to an inactive state.
    /// </summary>
    /// <param name="customerPackage">The customer package to clear.</param>
    private void ClearPackageDetails(CustomerPackage customerPackage)
    {
        customerPackage.PackageArabicName = null;
        customerPackage.PackageEnglishName = null;
        customerPackage.PackageId = null;
        customerPackage.PublishToOfPackage = null;
        customerPackage.PublishFromOfPackage = null;
        customerPackage.PriceOfPackage = null;
        customerPackage.DurationOfPackage = null;
        customerPackage.DectivationDateOfPackage = null;
        customerPackage.DescriptionOfPackage = null;
        customerPackage.IsCustomPackage = false;
        customerPackage.CustomerPackageStatus = CustomerPackageStatus.InActive;
    }
 

    /// <summary>
    /// Retrieves the subscription entity associated with a given organization.
    /// </summary>
    /// <param name="organization">The organization entity to look up the subscription for.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains the <see cref="Subscription"/> object if found; otherwise, null.
    /// </returns>
    private async Task<Subscription?> GetSubscriptionByOrganizationId(Organization organization)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            return await _subscriptionRepository.FirstOrDefaultAsync(s => s.Id == organization.SubscriptionId);
        }
    }
    /// <summary>
    /// Counts the number of packages related to a specific customer in a given organization.
    /// </summary>
    /// <param name="customerPackage">The customer package to use for filtering the count.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains the count of customer packages that belong to the same customer and organization.
    /// </returns>
    private async Task<int> CheckCountOfCustomerPackage(CustomerPackage? customerPackage)
    {
        var query = await _repository.GetQueryableAsync();
        int RelatedPackages = await query.CountAsync(p =>
            p.OrganizationId == customerPackage.OrganizationId &&
            p.CustomerId == customerPackage.CustomerId);
        return RelatedPackages;
    }

    /// <summary>
    /// Retrieves the currently authenticated customer based on the user's identity.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains the <see cref="Customer"/> entity if found; otherwise, null.
    /// </returns>
    private async Task<Customer> GetCustomerAsync()
    {
        return await _customerRepository.FirstOrDefaultAsync(c => c.IdentityUserId == CurrentUser.Id);
    }
    /// <summary>
    /// Retrieves a package entity by its ID.
    /// </summary>
    /// <param name="packageId">The ID of the package to retrieve.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the <see cref="Package"/> entity if found; otherwise, null.
    /// </returns>
    private async Task<Package> GetPackageAsync(int packageId)
    {
        return await _packageRepository.FirstOrDefaultAsync(p => p.Id == packageId);

    }
    /// <summary>
    /// Checks whether a customer package already exists based on package ID, organization, customer, and plate numbers.
    /// </summary>
    /// <param name="packageId">The ID of the package, or null for a custom package.</param>
    /// <param name="organizationId">The ID of the organization.</param>
    /// <param name="customerId">The ID of the customer.</param>
    /// <param name="ArabicPlateNumber">The Arabic plate number of the customer's vehicle.</param>
    /// <param name="EnglishPlateNumber">The English plate number of the customer's vehicle.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result is true if a matching active package exists; otherwise, false.
    /// </returns>
    private async Task<bool> CustomerPackageExistsAsync(int? packageId, int organizationId, Guid customerId, string ArabicPlateNumber, string EnglishPlateNumber)
    {
        var existingPackage = await _repository.AnyAsync(p =>
            ((packageId.HasValue && p.PackageId == packageId.Value) || (!packageId.HasValue && p.IsCustomPackage && p.PackageId == null)) &&
            p.OrganizationId == organizationId &&
            p.CustomerId == customerId &&
            p.ExpiryDate >= DateTime.Today &&
            p.CustomerPackageStatus == CustomerPackageStatus.Active &&
            p.ArPlateNumber == ArabicPlateNumber &&
            p.EnPlateNumber == EnglishPlateNumber
            );
        return existingPackage;
    }
    /// <summary>
    /// Retrieves an existing unassigned standard package (i.e., not a custom package and without a package ID) for a specific organization.
    /// </summary>
    /// <param name="organizationId">The ID of the organization.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the existing <see cref="CustomerPackage"/> if found; otherwise, null.
    /// </returns>
    private async Task<CustomerPackage?> GetExistingPackageAsync(int organizationId)
    {
        var query = await _repository.GetQueryableAsync();
        return await query.AsNoTracking()
            .FirstOrDefaultAsync(p => p.IsCustomPackage == false && p.PackageId == null && p.OrganizationId == organizationId);
    }
    /// <summary>
    /// Updates a given customer package in the database.
    /// </summary>
    /// <param name="customerPackage">The customer package to update.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="Result"/> indicating success or failure.
    /// </returns>
    private async Task<Result<CustomerPackage>> UpdateCustomerPackageAsync(CustomerPackage customerPackage)
    {
        var updatedPackage = await _repository.UpdateAsync(customerPackage, true);
        return updatedPackage != null ? Result.Success(updatedPackage) : Result.Error("Can't assign package to customer");
    }
    /// <summary>
    /// Inserts a new customer package into the database.
    /// </summary>
    /// <param name="customerPackage">The customer package to insert.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="Result"/> indicating success or failure.
    /// </returns>
    private async Task<Result<CustomerPackage>> InsertCustomerPackageAsync(CustomerPackage customerPackage)
    {
        var assignedPackage = await _repository.InsertAsync(customerPackage, true);
        return assignedPackage != null ? assignedPackage : Result.Error("Can't assign package to customer");
    }
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
    private static CustomerPackage CreateCustomerPackage(CustomerPackageDto customerPackageDto)
    {
        return new CustomerPackage
        {
            PackageId = customerPackageDto.PackageId,
            CustomerId = customerPackageDto.CustomerId,
            OrganizationId = customerPackageDto.OrganizationId,
            ExpiryDate = DateTime.Now.AddDays(customerPackageDto.DurationOfPackage.Value).AddMinutes(60 - DateTime.Now.AddDays(customerPackageDto.DurationOfPackage.Value).Minute),
            ActivationDateOfPackage = DateTime.Now,
            DectivationDateOfPackage = customerPackageDto.DectivationDateOfPackage,
            DescriptionOfPackage = customerPackageDto.DescriptionOfPackage,
            DurationOfPackage = customerPackageDto.DurationOfPackage,
            PriceOfPackage = customerPackageDto.PriceOfPackage,
            PublishFromOfPackage = customerPackageDto.PublishFromOfPackage,
            PublishToOfPackage = customerPackageDto.PublishToOfPackage,
            publishedOfPackage = customerPackageDto.publishedOfPackage,
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
    private static CustomerPackage CreateCustomerMobilePackage(Package PackageDto, Guid CustomerId, string arPlateNumber, string enPlateNumber)
    {
        return new CustomerPackage
        {
            PackageId = PackageDto.Id,
            CustomerId = CustomerId,
            OrganizationId = PackageDto.OrganizationId,
            ExpiryDate = DateTime.Now.AddDays(PackageDto.DurationOfPackage).AddMinutes(60 - DateTime.Now.AddDays(PackageDto.DurationOfPackage).Minute),
            ActivationDateOfPackage = DateTime.Now,
            DectivationDateOfPackage = PackageDto.DectivationDate,
            DescriptionOfPackage = PackageDto.Description,
            DurationOfPackage = PackageDto.DurationOfPackage,
            PriceOfPackage = PackageDto.Price,
            PublishFromOfPackage = PackageDto.PublishFrom,
            PublishToOfPackage = PackageDto.PublishTo,
            publishedOfPackage = PackageDto.published,
            CustomerCategory = PackageDto.CustomerCategory,
            PackageArabicName = PackageDto.ArabicName,
            PackageEnglishName = PackageDto.EnglishName,
            TenantId = PackageDto.TenantId,
            ArPlateNumber = arPlateNumber,
            EnPlateNumber = enPlateNumber,
            Vat = Math.Round(0.15m * PackageDto.Price, 2),

        };
    }
    #endregion

    #region UnUsed Methods 
    /// <summary>
    /// Deletes a customer package by its ID. 
    /// Performs a hard delete if the customer has more than one related package; 
    /// otherwise, clears the package details and marks it as inactive.
    /// </summary>
    /// <param name="id">The ID of the <see cref="CustomerPackage"/> to delete.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    /// <exception cref="EntityNotFoundException">
    /// Thrown if the customer package with the specified ID is not found.
    /// </exception>
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
    private PaymentCheckoutRequestDto RequestToPaymentCheckout(CustomerPackageDto customerPackageDto,Guid TenantId=default)
    {
        return new PaymentCheckoutRequestDto
        {
            OrganizationId = customerPackageDto!.OrganizationId,
            PaymentCategory = PaymentCategory.Package,
            CustomerPackageId = customerPackageDto.Id,
            PackageId = customerPackageDto.PackageId,
            PaymentMethod = PaymentMethod.Cash,
            Amount = PaymentUtilityCalculationts.GetPriceForPayment(customerPackageDto.PriceOfPackage.Value, customerPackageDto.Discount),
            CustomerId = customerPackageDto.CustomerId,
            ArPlateNumber = customerPackageDto.ArPlateNumber,
            EnPlateNumber = customerPackageDto.EnPlateNumber,
            TenantId = TenantId,
        };

    }


    #endregion

}
