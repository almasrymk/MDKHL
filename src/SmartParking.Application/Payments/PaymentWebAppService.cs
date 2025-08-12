
using Castle.Core.Resource;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using SmartParking.Cutomers;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Payments;
using SmartParking.Enums;
using SmartParking.Garages;
using SmartParking.Invoices;
using SmartParking.Organizations;
using SmartParking.Packages;
using SmartParking.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Payments;

public class PaymentWebAppService : ApplicationService, IPaymentWebAppService
{
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<Organization, int> _organizationRepository;
    private readonly IPackagesAppService _packagesAppService;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Invoice, int> _invoiceAppService;
    private readonly IOrganizationsAppService _organizationsAppService;
    private readonly IMyUserRepository _myUserRepository;

    public PaymentWebAppService(IRepository<Payment, Guid> paymentRepository, IDataFilter dataFilter, IPackagesAppService packagesAppService,
        IRepository<Customer, Guid> CustomerRepository,
        IRepository<Invoice, int> invoiceAppService, IOrganizationsAppService organizationsAppService, IRepository<Organization, int> organizationRepository, IMyUserRepository myUserRepository)
    {
        _paymentRepository = paymentRepository;
        _dataFilter = dataFilter;
        _packagesAppService = packagesAppService;
        _customerRepository = CustomerRepository;
        _invoiceAppService = invoiceAppService;
        _organizationsAppService = organizationsAppService;
        _organizationRepository = organizationRepository;
        _myUserRepository = myUserRepository;
    }
    #region public methods 
    public async Task<PagedResultDto<PaymentDto>> GetListMdkhlAsync(PagedAndSortedAndSearchResultRequestDto input,Guid? tenantId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _paymentRepository.GetQueryableAsync();
            var organizationQuery = await _organizationRepository.GetQueryableAsync();
            var customerQuery = await _customerRepository.GetQueryableAsync();
            var userQuery = await _myUserRepository.GetQueryableAsync();
            customerQuery = customerQuery.Include(x => x.IdentityUser);
            var result = from payment in query
                    join organization in organizationQuery on payment.OrganizationId equals organization.Id
                    join user in userQuery on payment.CreatorId equals user.Id
                    join customer in customerQuery on payment.CustomerId equals customer.Id
                    into customerList
                    from customer in customerList.DefaultIfEmpty()
                         select new PaymentDto()
                    {
                        Amount = payment.Amount,
                        CheckoutId = payment.CheckoutId,
                        CreationTime = payment.CreationTime.ToString(),
                        Currency = payment.Currency,
                        CustomerId = payment.CustomerId,
                        Id = payment.Id,
                        InvoiceId = payment.InvoiceId,
                        OrganizationId = payment.OrganizationId,
                        PaymentCategory = payment.PaymentCategory,
                        PaymentMethod = payment.PaymentMethod,
                        PaymentStatus = payment.PaymentStatus,
                        PackageId = payment.PackageId,
                        QrCodeInvoice = payment.QrCodeInvoice,
                        OrganizationArabicName = organization.ArabicName,
                        OrganizationEnglishName = organization.EnglishName,
                        PaymentNumber = payment.PaymentNumber,
                        PlateNumber = payment.EnPlateNumber,
                        TenantId = payment.TenantId,
                        Customers = customer,
                        Invoice = payment.Invoice  , 
                        CashierName = user.Name,
                        StatusZatca = organization.StatusZatca
                    };
            result = result.OrderByDescending(x => x.CreationTime);
            result = ApplyFilterTenant(tenantId, result);
            result = ApplySearchMdkhl(input, result);
            result = ApplySortMdkl(input, result);
            var totalCount = await AsyncExecuter.CountAsync(result);
            result = result.Skip(input.SkipCount).Take(input.MaxResultCount);
            var payments = await AsyncExecuter.ToListAsync(result);
           
            return new PagedResultDto<PaymentDto>
            {
                TotalCount = totalCount,
                Items = payments
            };

        }
    }
    public async Task<PagedResultDto<PaymentDto>> GetListOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input)
    {

            var query = await _paymentRepository.GetQueryableAsync();
            var customerQuery = await _customerRepository.GetQueryableAsync();
            var organizationQuery = await _organizationRepository.GetQueryableAsync();
            var userQuery = await _myUserRepository.GetQueryableAsync();
            customerQuery = customerQuery.Include(x => x.IdentityUser);
            var result = from payment in query
                         join organization in organizationQuery on payment.OrganizationId equals organization.Id
                         join user in userQuery on payment.CreatorId equals user.Id
                         join customer in customerQuery on payment.CustomerId equals customer.Id
                         into customerList
                         from customer in customerList.DefaultIfEmpty()
                         select new PaymentDto()
                         {
                             Amount = payment.Amount,
                             CheckoutId = payment.CheckoutId,
                             CreationTime = payment.CreationTime.ToString(),
                             Currency = payment.Currency,
                             CustomerId = payment.CustomerId,
                             Id = payment.Id,
                             InvoiceId = payment.InvoiceId,
                             OrganizationId = payment.OrganizationId,
                             PaymentCategory = payment.PaymentCategory,
                             PaymentMethod = payment.PaymentMethod,
                             PaymentStatus = payment.PaymentStatus,
                             PackageId = payment.PackageId,
                             QrCodeInvoice = payment.QrCodeInvoice,
                             PaymentNumber = payment.PaymentNumber,
                             PlateNumber = payment.EnPlateNumber,
                             TenantId = payment.TenantId,
                             Customers = customer,
                             Invoice = payment.Invoice,
                             CashierName = user.Name,
                             StatusZatca = organization.StatusZatca,
                             IsSentZatca = payment.IsSentZatca,
                             ErrorMessagesZatca = payment.ErrorMessagesZatca
                         };
            result = result.OrderByDescending(x => x.CreationTime);
            result = ApplySearch(input, result);
            result = ApplySort(input, result);
            var totalCount = await AsyncExecuter.CountAsync(result);
            result = result.Skip(input.SkipCount).Take(input.MaxResultCount);
            var payments = await AsyncExecuter.ToListAsync(result);
            return new PagedResultDto<PaymentDto>
            {
                TotalCount = totalCount,
                Items = payments
            };
       
    }

    public async Task<PaymentGetResponseDto> GetMdkhlAsync(Guid paymentId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var payment = await CheckPayment(paymentId);
            var paymentMap = ObjectMapper.Map<Payment, PaymentGetResponseDto>(payment);
            paymentMap.Organization = await CheckOrganization(payment);
            paymentMap.Invoice = await CheckInvoice(payment);
            paymentMap.Package = await CheckPackage(payment);
            paymentMap.CashierName = await GetCashierName(payment.CreatorId);
            paymentMap.StatusZatca = paymentMap.Organization?.StatusZatca ?? Status.Inactive;
            return paymentMap;
        }
    }

    public async Task<PagedResultDto<PaymentDto>> SearchMdkhlAsync(PagedAndSortedAndSearchResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            // Get the IQueryable<garage> from the repository
            var queryable = await _paymentRepository.GetQueryableAsync();

            // Apply search filtering
           // queryable = ApplySearch(input, queryable);
            if (!input.Sorting.IsNullOrEmpty())
            {
                if (input.Sorting.Contains("DESC"))
                {
                    var sorting = input.Sorting.Split(' ').ToList();
                    queryable = queryable.OrderBy(sorting[0] + " descending");
                }
                else
                {
                    queryable = queryable.OrderBy(input.Sorting);
                }

            }
            queryable = queryable.OrderByDescending(x => x.Id);
            // Get the total count of filtered records
            var totalCount = await AsyncExecuter.CountAsync(queryable);

            // Apply pagination
            queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
            // Execute the query and get the list of hotel entities
            var payments = await AsyncExecuter.ToListAsync(queryable);

            // Map the hotel entities to hotelDto objects
            var paymentResponseDtos = ObjectMapper.Map<List<Payment>, List<PaymentDto>>(payments);
            // Return the paginated result
            return new PagedResultDto<PaymentDto>(
                totalCount,
                paymentResponseDtos
            );
        }
    }
    public async Task<PagedResultDto<PaymentDto>> SearchOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input)
    {
        var organization = await _organizationsAppService.GetOrganizationForCurrentTenatAsync();
        using (_dataFilter.Disable<IMultiTenant>())
        {
            // Get the IQueryable<garage> from the repository
            var queryable = await _paymentRepository.GetQueryableAsync();
            queryable = queryable.Where(x => x.OrganizationId == organization.Id);
            // Apply search filtering
            if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
            {


                queryable = queryable.Where(or =>
                    or.Amount.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.PaymentMethod.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.PaymentStatus.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.PaymentCategory.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.Currency.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.Customers.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.Customers.IdentityUser.UserName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.Customers.IdentityUser.Name.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.Customers.CustomerCategory.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.PaymentNumber.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    (or.InvoiceId != null && or.Invoice.PlateNumber.ToLower().Contains(input.SearchingTerm.ToLower()))

                // full date-time or includes hour
                );

            }
            if (!input.Sorting.IsNullOrEmpty())
            {
                if (input.Sorting.Contains("DESC"))
                {
                    var sorting = input.Sorting.Split(' ').ToList();
                    queryable = queryable.OrderBy(sorting[0] + " descending");
                }
                else
                {
                    queryable = queryable.OrderBy(input.Sorting);
                }

            }
            queryable = queryable.OrderByDescending(x => x.Id);

            // Get the total count of filtered records
            var totalCount = await AsyncExecuter.CountAsync(queryable);

            // Apply pagination
            queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
            // Execute the query and get the list of hotel entities
            var payments = await AsyncExecuter.ToListAsync(queryable);

            // Map the hotel entities to hotelDto objects
            var paymentResponseDtos = ObjectMapper.Map<List<Payment>, List<PaymentDto>>(payments);
            // Return the paginated result
            return new PagedResultDto<PaymentDto>(
                totalCount,
                paymentResponseDtos
            );
        }
    }

    public async Task<PaymentDto> GetPaymentForInvoice(int invoiceId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var payment = await _paymentRepository.FirstOrDefaultAsync(x => x.InvoiceId == invoiceId);
            var paymentDto = ObjectMapper.Map<Payment, PaymentDto>(payment);
            paymentDto.CashierName = payment is not null ? await GetCashierName(payment.CreatorId) : null;
            return paymentDto;
        }
    }
    #endregion
    #region private methods 
    private static IQueryable<PaymentDto> ApplyFilterTenant(Guid? tenantId, IQueryable<PaymentDto> query)
    {
        if (tenantId.HasValue)
            query = query.Where(x => x.TenantId == tenantId);
        return query;
    }

    private static IQueryable<PaymentDto> ApplySort(PagedAndSortedResultRequestDto input, IQueryable<PaymentDto> query)
    {
        if (!input.Sorting.IsNullOrEmpty())
        {
            if (input.Sorting.Contains("DESC"))
            {
                var sorting = input.Sorting.Split(' ').ToList();
                query = query.OrderBy(sorting[0] + " descending");
            }
            else
            {
                query = query.OrderBy(input.Sorting);
            }

        }

        return query;
    }   
    
    private static IQueryable<PaymentDto> ApplySortMdkl(PagedAndSortedResultRequestDto input, IQueryable<PaymentDto> query)
    {
        if (!input.Sorting.IsNullOrEmpty())
        {
            if (input.Sorting.Contains("DESC"))
            {
                var sorting = input.Sorting.Split(' ').ToList();
                query = query.OrderBy(sorting[0] + " descending");
            }
            else
            {
                query = query.OrderBy(input.Sorting);
            }

        }

        return query;
    }

   
    private async Task<PackageDto> CheckPackage(Payment payment)
    {
        int? packageId = payment.PackageId ;
        if (packageId == null || packageId == 0)
            return null; 
       return await _packagesAppService.GetAsync(packageId ?? 0);
    }

    private async Task<InvoiceDto> CheckInvoice(Payment payment)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            int?invoiceId = payment.InvoiceId;
            if (invoiceId == null)
                return null;
            var query = await _invoiceAppService.GetQueryableAsync();
            var invoiceEntity=  await query.FirstOrDefaultAsync(x => x.Id == invoiceId);
            return ObjectMapper.Map<Invoice, InvoiceDto>(invoiceEntity);
        }
        
    }

    private async Task<OrganizationDto> CheckOrganization(Payment payment)
    {
        var organization = await _organizationsAppService.GetAsync(payment.OrganizationId);
        if (organization == null)
            throw new Exception("Organization not Found");

        return organization;
    }

    private async Task<Payment> CheckPayment(Guid paymentId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var payment = await _paymentRepository.FirstOrDefaultAsync(x => x.Id == paymentId);
            if (payment == null)
                throw new Exception("payment not Found");
            return payment;
        }

    }

    private static IQueryable<PaymentDto> ApplySearch(PagedAndSortedAndSearchResultRequestDto input, IQueryable<PaymentDto> queryable)
    {
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {
            queryable = queryable.Where(or =>
                or.Amount.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.PaymentMethod.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.PaymentStatus.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.PaymentCategory.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Currency.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Customers.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Customers.IdentityUser.UserName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Customers.IdentityUser.Name.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Customers.CustomerCategory.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.PaymentNumber.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Invoice.PlateNumber.ToLower().Contains(input.SearchingTerm.ToLower())  ||
                or.Invoice.Id.ToString().Contains(input.SearchingTerm)  ||
                or.CashierName.ToLower().Contains(input.SearchingTerm.ToLower())
            );
        }
        return queryable;
    }

    private static IQueryable<PaymentDto> ApplySearchMdkhl(PagedAndSortedAndSearchResultRequestDto input, IQueryable<PaymentDto> queryable)
    {
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {
            queryable = queryable.Where(or =>
                or.Amount.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.PaymentMethod.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.PaymentStatus.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.PaymentCategory.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Currency.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Customers.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Customers.IdentityUser.Name.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Customers.CustomerCategory.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Invoice.Id.ToString().Contains(input.SearchingTerm) ||
                or.PaymentNumber.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.PlateNumber.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.OrganizationArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.OrganizationEnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.CashierName.ToLower().Contains(input.SearchingTerm.ToLower())

            // full date-time or includes hour
            );
        }
        return queryable;
    }
    private async Task<string> GetCashierName(Guid? creatorId)
    {
        var user = await _myUserRepository.GetAsync((Guid)creatorId!);
        return user.Name;
    }
    #endregion
}
