using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Payments;
using SmartParking.Enums;
using SmartParking.Filters;
using SmartParking.Localization;
using SmartParking.QRCode;
using SmartParking.Results;
using SmartParking.Utilities;
using SmartParking.Validations;
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

namespace SmartParking.Invoices;
//[Authorize]
public class InvoiceAppService : CrudAppService<Invoice, InvoiceDto, int, PagedAndSortedResultRequestDto, CreateUpdateInvoiceDto>, IInvoiceAppService 
{
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<Invoice, int> _invoiceRepository;
    private readonly IRepository<Organization, int> _organizationsRepository;
    private readonly IQRCodeAppService _qrCodeAppService;
    private readonly IInvoiceUtilityAppService _invoiceUtility;
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IRepository<GarageTransaction, int> _garageTransactionRepository;
    private readonly IRepository<Dispenser, int> _dispenserRepository;

    private string LastInvoicePageUrl = "https://localhost:44322/checkout-extension/";

    public InvoiceAppService(IRepository<Invoice, int> repository, 
        IDataFilter dataFilter, 
        IRepository<Organization, int> organizationsRepository, 
        IInvoiceUtilityAppService invoiceUtility, 
        IRepository<Payment, Guid> paymentRepository, 
        IQRCodeAppService qrCodeAppService, 
        IRepository<GarageTransaction, int> garageTransactionRepository,
        IRepository<Dispenser, int> dispenserRepository)
    : base(repository)
    {
        _dataFilter = dataFilter;
        _invoiceRepository = repository;
        _organizationsRepository = organizationsRepository;
        _invoiceUtility = invoiceUtility;
        _paymentRepository = paymentRepository;
        _qrCodeAppService = qrCodeAppService;
        _garageTransactionRepository = garageTransactionRepository;
        _dispenserRepository = dispenserRepository;
    }
    #region public methods 
    /// <inheritdoc/>

    public override async Task<InvoiceDto> CreateAsync(CreateUpdateInvoiceDto input)
    {
        Organization organization = null;
        using (_dataFilter.Disable<IMultiTenant>())
        {
            organization = await _organizationsRepository.GetAsync(input.OrganizationId);
            if (organization is null)
            {
                throw new Exception("Unable to retrive Organization");
            }
        }
        input.TenantId = organization.TenantId;

        if (CurrentUser.IsInRole(RolesType.Mdkhl.ToString()) || CurrentUser.IsInRole("admin"))
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                return await base.CreateAsync(input);
            }
        }
        else
            return await base.CreateAsync(input);
    }
    [Authorize]
    /// <inheritdoc/>

    public override async Task<PagedResultDto<InvoiceDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _invoiceRepository.GetQueryableAsync();
        if (!input.Sorting.IsNullOrEmpty())
        {
            // Determine if the sorting should be ascending or descending
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
        using (DataFilter.Disable<IMultiTenant>())
        {
            var invoices = await base.GetListAsync(input);

            return invoices;
        }
    }

    [Authorize]
    /// <inheritdoc/>

    public override async Task<InvoiceDto> GetAsync(int id)
    {
        if (CurrentTenant.Id == null)
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                return await base.GetAsync(id);
            }
        }
        else
            return await base.GetAsync(id);
    }

    ///<inheritdoc/>
    [Authorize]
    public async Task<PagedResultDto<InvoiceDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        // Get the IQueryable<garage> from the repository
        var queryable = await _invoiceRepository.GetQueryableAsync();
        // Apply search filtering
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {
            queryable = queryable.Where(or =>
                or.Id.ToString().Contains(input.SearchingTerm.ToLower()) ||
                or.PlateNumber.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.InvoicePrice.ToString().Contains(input.SearchingTerm.ToLower()) ||
                or.CustomerArName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.CustomerEnName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.InvoiceType.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.InvoiceTime.ToString()!.Contains(input.SearchingTerm.ToLower()) ||
                or.TotalHours.ToString().Contains(input.SearchingTerm.ToLower()) ||
                or.Organization.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Organization.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.TicketId.Contains(input.SearchingTerm)
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
        var invoices = await AsyncExecuter.ToListAsync(queryable);
        // Map the hotel entities to hotelDto objects
        var invoiceDto = ObjectMapper.Map<List<Invoice>, List<InvoiceDto>>(invoices);
        // Return the paginated result
        return new PagedResultDto<InvoiceDto>(
            totalCount,
            invoiceDto
        );
    }

    [Authorize]
    /// <inheritdoc/>

    public async Task<PagedResultDto<InvoiceDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        // Get the IQueryable<garage> from the repository
        var queryable = await _invoiceRepository.GetQueryableAsync();
        // Apply filtering
        var filterExpression = BuildExpression<Invoice>.BuildFilterExpression(input.filters);
        if (filterExpression is not null)
        {
            queryable = queryable.Where(filterExpression);
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
        // Execute the query and get the list of garages entities
        var invoices = await AsyncExecuter.ToListAsync(queryable);
        // Map the garages entities to garagesDto objects
        var invoicesDto = ObjectMapper.Map<List<Invoice>, List<InvoiceDto>>(invoices);
        // Return the paginated result
        return new PagedResultDto<InvoiceDto>(
            totalCount,
            invoicesDto
        );
    }
    [Authorize]
    /// <inheritdoc/>

    public override async Task DeleteAsync(int id)
    {
        var invoice = await _invoiceRepository.FindAsync(id);
        if (invoice is null)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("Invoice") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
        }
        await base.DeleteAsync(id);
        await CurrentUnitOfWork.SaveChangesAsync();
    }
    /// <inheritdoc/>
    public async Task<Result<string>> GetInvoiceQRCodeForOrganization(int invoiceId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var invoice = await GetAsync(invoiceId);
            if (invoice is null)
            {
                return Result.NotFound();
            }
            string? qrCode = await GetQrCodeAsync(invoice);

            return await _qrCodeAppService.CreateQRCOdeImageAsync(qrCode);
        }
    }
    /// <inheritdoc/>

    public async Task<Result<string>> GetInvoiceQRCodeForMDKHL(int invoiceId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var invoice = await GetAsync(invoiceId);
            if (invoice is null)
            {
                return Result.NotFound();
            }
            string? qrCode = await GetQrCodeAsync(invoice);
            var result = await _qrCodeAppService.CreateQRCOdeImageAsync( qrCode);
            return result;
        }
    }
    
    /// <inheritdoc/>
	public async Task<PagedResultDto<InvoiceDto>> GetAllInvoicesByOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId)
    {
        if (CurrentTenant.Id == null)
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                return await GetAll(input, null);
            }
        }
        return await GetAll(input, tenantId);

    }

    private async Task<PagedResultDto<InvoiceDto>> GetAll(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId)
    {
        var queryable = await _invoiceRepository.GetQueryableAsync();
        if (tenantId.HasValue)
        {
            queryable = queryable.Where(t => t.TenantId == tenantId);
        }
        if (!input.Sorting.IsNullOrEmpty())
        {
            if (input.Sorting.Contains("DESC"))
            {
                var sorting = input.Sorting.Split(' ').ToList();
                queryable = queryable.OrderBy(sorting[0] + " descending");
                if (input.Sorting.Contains("InvoiceType"))
                    queryable = queryable = queryable.OrderBy(sorting[0] + " descending").ThenByDescending(_ => _.IsPackage);
            }
            else
            {
                queryable = queryable.OrderBy(input.Sorting);
                if (input.Sorting.Contains("InvoiceType"))
                    queryable = queryable = queryable.OrderBy(input.Sorting).ThenBy(_ => _.IsPackage);
            }
        }
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {
            queryable = queryable.Where(or =>
                or.Id.ToString().Contains(input.SearchingTerm.ToLower()) ||
                or.PlateNumber.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.InvoicePrice.ToString().Contains(input.SearchingTerm.ToLower()) ||
                or.InvoiceNumber.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.CustomerArName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.CustomerEnName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.InvoiceType.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.InvoiceTime.ToString()!.Contains(input.SearchingTerm.ToLower()) ||
                or.TotalHours.ToString().Contains(input.SearchingTerm.ToLower()) ||
                or.Organization.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Organization.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower())  ||
                or.TicketId.Contains(input.SearchingTerm)
            );
        }
        var totalCount = await AsyncExecuter.CountAsync(queryable);
        queryable = queryable
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);
        var items = await AsyncExecuter.ToListAsync(queryable);
        // Map entities to DTOs
        var invoicesDtos = ObjectMapper.Map<List<Invoice>, List<InvoiceDto>>(items);
        // Return paged result
        return new PagedResultDto<InvoiceDto>(totalCount, invoicesDtos);
    }

    /// <inheritdoc/>

    public async Task<Result<InvoiceDto>> GetLatestInvoiceForCustomerInOrganizationGarage(string PlateNumber, int OrganizationId, int GarageId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _invoiceRepository.GetQueryableAsync();
            query = query.Where(inv => inv.PlateNumber == PlateNumber && inv.GarageId == GarageId && inv.OrganizationId == OrganizationId);
            query = query.OrderByDescending(inv => inv.InvoiceTime);
            var invoice = await AsyncExecuter.FirstOrDefaultAsync(query);
            var invoiceDto = ObjectMapper.Map<Invoice, InvoiceDto>(invoice);
            return Result.Success(invoiceDto);
        }
    }
    /// <inheritdoc/>

    public async Task<PagedResultDto<InvoiceDto>> GetInvoiceForCustomer(Guid customerId, PagedResultRequestDto input)
    {
        var query = await _invoiceRepository.GetQueryableAsync();
        query = query.Where(inv => inv.CustomerId == customerId);
        var totalCount = await AsyncExecuter.CountAsync(query);
        query = query.Skip(input.SkipCount).Take(input.MaxResultCount);
        var invoice = await AsyncExecuter.ToListAsync(query);
        var invoiceDto = ObjectMapper.Map<List<Invoice>, List<InvoiceDto>>(invoice);
        return new PagedResultDto<InvoiceDto>
        {
            Items = invoiceDto,
            TotalCount = totalCount
        };
    }
    /// <inheritdoc/>

    public async Task<Result<List<InvoiceDto>>> GetUnpaidInvoiceForKeeperInHisOrganization(int OrganizationId, string plateNumber)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _invoiceRepository.GetQueryableAsync();
            query = query.Where(inv => inv.OrganizationId == OrganizationId && inv.PlateNumber == plateNumber && inv.InvoiceType == InvoiceType.UnPaid);
            var invList = await AsyncExecuter.ToListAsync(query);
            var invDtos = ObjectMapper.Map<List<Invoice>, List<InvoiceDto>>(invList);
            return Result.Success(invDtos);
        }
    }
    /// <inheritdoc/>

    public async Task<InvoiceDto?> GetLatestInvoiceForOrganization()
    {
        var query = await _invoiceRepository.GetQueryableAsync();
        query = query.Where(x => x.InvoiceType == InvoiceType.UnPaid
        && x.InvoicePrice > 0);

        query = query.OrderByDescending(x => x.CreationTime);


        var invoice = await query.FirstOrDefaultAsync();
        if (invoice == null)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("VehicleNotFoundAtGate"));
        }
        return ObjectMapper.Map<Invoice, InvoiceDto>(invoice);
    }
    
    /// <inheritdoc/>

    public async Task<PagedResultDto<InvoiceDto>> SearchForMDKHL(PagedAndSortedAndSearchResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await Search(input);
        }
    }
    /// <inheritdoc/>

    public async Task UpdateDiscountForInvoice(int invoiceId, decimal discount)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var invoice = await _invoiceRepository.FirstOrDefaultAsync(x => x.Id == invoiceId);
            if (invoice == null)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("Invoice") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation") + LocalizationHelper.GetLocalizedString("AtGate"));
            }
            var result = NumbersValidations.ValidateInvoice(discount, invoice.InvoicePrice);
            if (result == null)
            {

                invoice.Discount = discount;
                invoice.InvoiceType = InvoiceType.Paid;
                await _invoiceRepository.UpdateAsync(invoice);
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            else
            {
                throw new Exception(result);
            }
        }
    }

    /// <inheritdoc/>
    public async Task<string> QrCodeForInvoicePage(string watcherCode,string domain)
    {
        string qrText = $"{domain}checkout-extension/";
        return await _qrCodeAppService.CreateQRCOdeImageAsync(qrText);
    }
    #endregion
    #region private methods
    /// <summary>
    /// Asynchronously retrieves the QR code associated with the specified invoice.
    /// </summary>
    /// <param name="invoice">An instance of <see cref="InvoiceDto"/> representing the invoice for which the QR code is being fetched.</param>
    /// <returns>A string representing the QR code if found, or <c>null</c> if no QR code is associated with the invoice.</returns>
    private async Task<string?> GetQrCodeAsync(InvoiceDto? invoice)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var paymentQuery = await _paymentRepository.GetQueryableAsync();
            string? qrCode = await paymentQuery
                    .Where(p => p.InvoiceId == invoice.Id)
                    .Select(p => p.QrCodeInvoice)
                    .FirstOrDefaultAsync();
            return qrCode;
        }
    }
    #endregion

}
