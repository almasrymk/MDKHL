
using Microsoft.EntityFrameworkCore;
using SmartParking.Cutomers;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Payments;
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
    private readonly IPackagesAppService _packagesAppService;
    private readonly IRepository<Invoice, int> _invoiceAppService;
    private readonly IOrganizationsAppService _organizationsAppService;

    public PaymentWebAppService(IRepository<Payment,Guid> paymentRepository,IDataFilter dataFilter,IPackagesAppService packagesAppService,IRepository<Invoice,int> invoiceAppService,IOrganizationsAppService organizationsAppService)
    {
        _paymentRepository = paymentRepository;
        _dataFilter = dataFilter;
        _packagesAppService = packagesAppService;
        _invoiceAppService = invoiceAppService;
        _organizationsAppService = organizationsAppService;
    }

    public async Task<PagedResultDto<PaymentDto>> GetListMdkhlAsync(PagedAndSortedResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _paymentRepository.GetQueryableAsync();
            query = query.Include(x => x.Customers);
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
    

    public async Task<PagedResultDto<PaymentDto>> GetListOrganizationAsync(PagedAndSortedResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var organization = await _organizationsAppService.GetOrganizationForCurrentTenat();
            var query = await _paymentRepository.GetQueryableAsync();
            query = query.Include(x => x.Customers);
            query = query.Where(x => x.OrganizationId == organization.Id);
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
        else
            return await _packagesAppService.GetAsync(packageId ?? 0);
    }

    private async Task<InvoiceDto> CheckInvoice(Payment payment)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            int? invoiceId = payment.InvoiceId;
            if (invoiceId == null)
                return null;
                
            var invoice = await _invoiceAppService.GetListAsync();
            // ObjectMapper.Map<Invoice, InvoiceDto>(invoice);
            return null;
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
            if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
            {


                queryable = queryable.Where(or =>
                    or.Amount.ToString().ToLower().Contains(input.SearchingTerm.ToLower())||
                    or.Currency.ToLower().Contains(input.SearchingTerm.ToLower()) 
                    
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
        var organization = await _organizationsAppService.GetOrganizationForCurrentTenat();
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
                    or.Currency.ToLower().Contains(input.SearchingTerm.ToLower())

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
}
