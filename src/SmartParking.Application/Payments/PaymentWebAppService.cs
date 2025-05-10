
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

    public PaymentWebAppService(IRepository<Payment,Guid> paymentRepository,IDataFilter dataFilter,IPackagesAppService packagesAppService,
        IRepository<Customer, Guid> CustomerRepository,
        IRepository<Invoice,int> invoiceAppService,IOrganizationsAppService organizationsAppService,IRepository<Organization, int> organizationRepository)
    {
        _paymentRepository = paymentRepository;
        _dataFilter = dataFilter;
        _packagesAppService = packagesAppService;
        _customerRepository = CustomerRepository;
        _invoiceAppService = invoiceAppService;
        _organizationsAppService = organizationsAppService;
        _organizationRepository = organizationRepository;
    }

    public async Task<PagedResultDto<PaymentDto>> GetListMdkhlAsync(PagedAndSortedAndSearchResultRequestDto input,Guid? tenantId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _paymentRepository.GetQueryableAsync();
            var organizationQuery = await _organizationRepository.GetQueryableAsync();
            var customerQuery = await _customerRepository.GetQueryableAsync();
            customerQuery = customerQuery.Include(x => x.IdentityUser);
            var result = from payment in query
                    join organization in organizationQuery on payment.OrganizationId equals organization.Id
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
                        Invoice = payment.Invoice

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

    private static IQueryable<PaymentDto> ApplyFilterTenant(Guid? tenantId, IQueryable<PaymentDto> query)
    {
        if (tenantId.HasValue)
            query = query.Where(x => x.TenantId == tenantId);
        return query;
    }

    private static IQueryable<Payment> ApplySort(PagedAndSortedResultRequestDto input, IQueryable<Payment> query)
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

    public async Task<PagedResultDto<PaymentDto>> GetListOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var organization = await _organizationsAppService.GetOrganizationForCurrentTenatAsync();
            var query = await _paymentRepository.GetQueryableAsync();
            query =query.Where(x => x.OrganizationId == organization.Id);
            query=query.OrderByDescending(x=>x.CreationTime);
            query = query.Include(x => x.Customers).ThenInclude(c=>c.IdentityUser);
            query = query.Include(x => x.Invoice);
            query = ApplySearch(input, query);
            query = ApplySort(input, query);
            var totalCount = await AsyncExecuter.CountAsync(query);
            query = query.Skip(input.SkipCount).Take(input.MaxResultCount);
            var payments = await AsyncExecuter.ToListAsync(query);
            var paymentDtos = ObjectMapper.Map<List<Payment>, List<PaymentDto>>(payments);

            return new PagedResultDto<PaymentDto>
            {
                TotalCount = totalCount,
                Items = paymentDtos
            };
        }
        
    }

    public async Task<PaymentGetResponseDto> GetMdkhlAsync(Guid paymentId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var payment = await CheckPayment(paymentId);
            var paymentMap = ObjectMapper.Map<Payment, PaymentGetResponseDto>(payment);
            paymentMap.Organization = await CheckOrganization(payment);
            paymentMap.Invoice = await CheckInvoice(payment);
            paymentMap.Package=await CheckPackage(payment);
            return paymentMap;

        }
    }

    private async Task<PackageDto> CheckPackage(Payment payment)
    {
        int? packageId = payment.PackageId ;
        if (packageId == null)
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

    public async Task<PagedResultDto<PaymentDto>> SearchMdkhlAsync(PagedAndSortedAndSearchResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            // Get the IQueryable<garage> from the repository
            var queryable = await _paymentRepository.GetQueryableAsync();

            // Apply search filtering
            queryable = ApplySearch(input, queryable);
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

    private static IQueryable<Payment> ApplySearch(PagedAndSortedAndSearchResultRequestDto input, IQueryable<Payment> queryable)
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
                or.Invoice.PlateNumber.ToLower().Contains(input.SearchingTerm.ToLower()) 


            // full date-time or includes hour
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
                or.PaymentNumber.ToLower().Contains(input.SearchingTerm.ToLower()) || 
                or.PlateNumber.ToLower().Contains(input.SearchingTerm.ToLower()) 


            // full date-time or includes hour
            );

        }

        return queryable;
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
                    (or.InvoiceId !=null && or.Invoice.PlateNumber.ToLower().Contains(input.SearchingTerm.ToLower()))

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
            return ObjectMapper.Map<Payment,PaymentDto>(await _paymentRepository.FirstOrDefaultAsync(x => x.InvoiceId == invoiceId));
        }
    }
}
