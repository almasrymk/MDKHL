using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QRCoder;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Payments;
using SmartParking.Enums;
using SmartParking.Results;
using SmartParking.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Invoices;
//[Authorize]
public class InvoiceAppService : CrudAppService<Invoice, InvoiceDto, int, PagedAndSortedResultRequestDto, CreateUpdateInvoiceDto>, IInvoiceAppService
{
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<Invoice, int> _invoiceRepository;
    private readonly IRepository<Organization, int> _organizationsRepository;
    private string folderQrImage = "QRCodeImage";
    private string folderImages = "image";
    private readonly IInvoiceUtility _invoiceUtility;
    private readonly IRepository<Payment, Guid> _paymentRepository;

    public InvoiceAppService(IRepository<Invoice, int> repository, IDataFilter dataFilter, IRepository<Organization, int> organizationsRepository, IInvoiceUtility invoiceUtility , IRepository<Payment, Guid> paymentRepository)
    : base(repository)
    {
        _dataFilter = dataFilter;
        _invoiceRepository = repository;
        _organizationsRepository = organizationsRepository;
        _invoiceUtility = invoiceUtility;
        _paymentRepository = paymentRepository;
    }

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
    public override async Task<InvoiceDto> GetAsync(int id)
    {
        if (CurrentTenant.Id==null)
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
                or.InvoicePrice.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.CustomerArName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.CustomerEnName.ToLower().Contains(input.SearchingTerm.ToLower()) |
                or.InvoiceType.ToString().ToLower().StartsWith(input.SearchingTerm.ToLower()) 
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
        queryable = queryable.OrderByDescending(x=>x.Id);
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
        Console.WriteLine(invoicesDto.Count);
        // Return the paginated result
        return new PagedResultDto<InvoiceDto>(
            totalCount,
            invoicesDto
        );
    }
    [Authorize]
    public override async Task DeleteAsync(int id)
    {
        var invoice = await _invoiceRepository.FindAsync(id);

        if (invoice is null)
        {
            throw new EntityNotFoundException(typeof(VehicleModel), id);
        }


        await base.DeleteAsync(id);
    }

 

    public async Task<Result<string>> GetInvoiceQRCodeForOrganization(int invoiceId)
    {
        var invoice = await _invoiceRepository.GetAsync(invoiceId);
        if (invoice is null)
        {
            return Result.NotFound();
        }
        string? qrCode = await GetQrCodeAsync(invoice);

        return await GetImage(invoice, qrCode);
    }

    private async Task<string?> GetQrCodeAsync(Invoice? invoice)
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

    public async Task<Result<string>> GetInvoiceQRCodeForMDKHL(int invoiceId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var invoice = await _invoiceRepository.GetAsync(invoiceId);
            if (invoice is null)
            {
                return Result.NotFound();
            }
            string? qrCode = await GetQrCodeAsync(invoice);
            var result = await GetImage(invoice, qrCode);
            return result;
        }
    }

    [RemoteService(false)]

    public async Task<Result<string>> GetImage(Invoice? invoice, string? qrCodedto)
    {
        //var text = Convert.FromBase64String(qrCodedto.QRCode);
        if (qrCodedto.IsNullOrEmpty())
            return " ";
        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())

        using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodedto, QRCodeGenerator.ECCLevel.Q))
        using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
        {

            byte[] qrCodeImage = qrCode.GetGraphic(20);


            string imageDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderQrImage, folderImages);

            if (!Directory.Exists(imageDirectoryPath))
            {
                Directory.CreateDirectory(imageDirectoryPath);
            }
            string fileName = $"{Guid.NewGuid()}{DateTime.Now:yyyyMMddHHmmssfff}.png";
            string fullPath = Path.Combine(imageDirectoryPath, fileName);


            await File.WriteAllBytesAsync(fullPath, qrCodeImage);

            Console.WriteLine($"Image saved successfully at {fullPath}");

            return $"{folderQrImage}/{folderImages}/{fileName}";
        }
    }

    /// <inheritdoc/>
	public async Task<PagedResultDto<InvoiceDto>> GetAllInvoicesByOrganizationAsync(PagedAndSortedResultRequestDto input, Guid? tenantId)
    {
        var queryable = await _invoiceRepository.GetQueryableAsync();

        queryable = queryable.Where(t => t.TenantId == tenantId);

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
        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var items = await AsyncExecuter.ToListAsync(
            queryable
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        // Map entities to DTOs
        var incoicesDtos = ObjectMapper.Map<List<Invoice>, List<InvoiceDto>>(items);

        // Return paged result
        return new PagedResultDto<InvoiceDto>(totalCount, incoicesDtos);
    }

    public async Task<Result<InvoiceDto>> GetLatestInvoiceForCustomerInOrganizationGarage(string PlateNumber, int OrganizationId, int GarageId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _invoiceRepository.GetQueryableAsync();
            query=query.Where(inv=>inv.PlateNumber == PlateNumber && inv.GarageId==GarageId && inv.OrganizationId == OrganizationId);
            var invoice = await AsyncExecuter.FirstOrDefaultAsync(query);
            var invoiceDto = ObjectMapper.Map<Invoice,InvoiceDto>(invoice);
            return Result.Success(invoiceDto);


        }
    }

    public async Task<List<InvoiceDto>> GetInvoiceForCustomer(Guid customerId)
    {

            var query = await _invoiceRepository.GetQueryableAsync();
            query = query.Where(inv => inv.CustomerId == customerId);
            var invoice = await AsyncExecuter.ToListAsync(query);
            var invoiceDto = ObjectMapper.Map<List<Invoice>, List<InvoiceDto>>(invoice);
            return invoiceDto;
    }

    public async Task<Result<List<InvoiceDto>>> GetUnpaidInvoiceForKeeperInHisOrganization(int OrganizationId,string plateNumber)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

            var query=await _invoiceRepository.GetQueryableAsync();
            query=query.Where(inv=>inv.OrganizationId== OrganizationId && inv.PlateNumber==plateNumber&&inv.InvoiceType==InvoiceType.UnPaid);
            var invList=await AsyncExecuter.ToListAsync(query);
            var invDtos = ObjectMapper.Map<List<Invoice>, List<InvoiceDto>>(invList);
            return Result.Success(invDtos);

        }
    }
}
