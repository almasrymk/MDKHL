using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QRCoder;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Results;
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
    private readonly HttpClient _httpClient;
    private string folderQrImage = "QRCodeImage";
    private string folderImages = "image";

    public InvoiceAppService(IRepository<Invoice, int> repository, IDataFilter dataFilter, IRepository<Organization, int> organizationsRepository, HttpClient httpClient)
    : base(repository)
    {
        _dataFilter = dataFilter;
        _invoiceRepository = repository;
        _organizationsRepository = organizationsRepository;
        _httpClient = httpClient;
    }

    public override async Task<InvoiceDto> CreateAsync(CreateUpdateInvoiceDto input)
    {
        var organization = await _organizationsRepository.GetAsync(input.OrganizationId);
        input.TenantId = organization.TenantId;
        if (input.InvoiceNumber is null)
        {
            input.InvoiceNumber = await GenerateInvoiceNumberAsync(input.OrganizationId);
        }

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
        if (CurrentTenant.Id == null)
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                return await base.GetListAsync(input);
            }
        }

        return await base.GetListAsync(input);
    }

    [Authorize]
    public override async Task<InvoiceDto> GetAsync(int id)
    {
        if (CurrentUser.IsInRole(RolesType.Mdkhl.ToString()) || CurrentUser.IsInRole("admin"))
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
                or.InvoiceNumber.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.PlateNumber.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.InvoicePrice.ToString().ToLower().Contains(input.SearchingTerm.ToLower())
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

    [RemoteService(IsEnabled = false)]

    public async Task<string> GenerateInvoiceNumberAsync(int organizationId)
    {
        var organization = await _organizationsRepository.GetAsync(organizationId);
        var currentYearSuffix = DateTime.Now.Year % 100;  // Last 2 digits of the year
        var prefix = organization.EnglishName.Substring(0, 3).ToUpper();

        // Fetch the last invoice for the current year and organization
        var invoices = await _invoiceRepository.GetListAsync(i => i.OrganizationId == organizationId);

        var lastInvoice = invoices
        .Where(i => i.CreationTime.Year == DateTime.Now.Year)
        .OrderByDescending(i => i.InvoiceNumber)
        .FirstOrDefault();


        int nextInvoiceSequence = 1;

        if (lastInvoice != null)
        {
            // Extract the last 3 digits of the previous invoice number (e.g., "ABC23-001")
            var lastSequenceString = lastInvoice.InvoiceNumber[^5..];
            if (int.TryParse(lastSequenceString, out int lastSequence))
            {
                nextInvoiceSequence = lastSequence + 1;
            }
        }

        // Format the sequence as a three-digit number with leading zeros
        var formattedSequence = nextInvoiceSequence.ToString().PadLeft(5, '0');

        // Combine all parts to form the final invoice number
        return $"{prefix}{currentYearSuffix}{formattedSequence}";
    }

    public async Task<Result<string>> GetInvoiceQRCodeForOrganization(int invoiceId)
    {
        string? token = await GetToken();
        var invoice = await _invoiceRepository.GetAsync(invoiceId);
        if (invoice is null)
        {
            return Result.NotFound();
        }
        QRCodeDto qrCodedto = await GetQrCode(token, invoice);
        return await GetImage(invoice, qrCodedto);
    }
    public async Task<Result<string>> GetInvoiceQRCodeForMDKHL(int invoiceId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            string? token = await GetToken();
            var invoice = await _invoiceRepository.GetAsync(invoiceId);
            if (invoice is null)
            {
                return Result.NotFound();
            }
            QRCodeDto qrCodedto = await GetQrCode(token, invoice);
            var result = await GetImage(invoice, qrCodedto);
            return result;
        }
    }

    private async Task<Result<string>> GetImage(Invoice? invoice, QRCodeDto qrCodedto)
    {
        //var text = Convert.FromBase64String(qrCodedto.QRCode);
        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())

        using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodedto.QRCode, QRCodeGenerator.ECCLevel.Q))
        using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
        {

            byte[] qrCodeImage = qrCode.GetGraphic(20);


            string imageDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderQrImage, folderImages);

            if (!Directory.Exists(imageDirectoryPath))
            {
                Directory.CreateDirectory(imageDirectoryPath);
            }
            string fileName = $"{invoice.InvoiceNumber}.{"png"}";
            string fullPath = Path.Combine(imageDirectoryPath, fileName);


            await File.WriteAllBytesAsync(fullPath, qrCodeImage);

            Console.WriteLine($"Image saved successfully at {fullPath}");

            return $"{folderQrImage}/{folderImages}/{fileName}";
        }
    }

    private async Task<QRCodeDto> GetQrCode(string? token, Invoice? invoice)
    {
        decimal percentage = invoice.InvoicePrice * 0.14m;
        var oragnization = await _organizationsRepository.GetAsync(invoice.OrganizationId);
        //"12-25-2024"
        var qR = new
        {
            sellerName = oragnization.EnglishName,
            vatRegistration = oragnization.CommercialRegistrationNumber,
            timeStamp = DateTime.Now.ToString("MM-dd-yyyy"),
            invoiceAmount = invoice.InvoicePrice.ToString(),
            vatAmount = percentage.ToString()
        };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var contentQR = new StringContent(JsonConvert.SerializeObject(qR), Encoding.UTF8, "application/json");
        HttpResponseMessage responseQR = await _httpClient.PostAsync("https://zatca.mazeedit.com/api/Invoices/V1/QRCode", contentQR);

        responseQR.EnsureSuccessStatusCode();

        var resultResponseQR = await responseQR.Content.ReadAsStringAsync();

        var jsonQR = JObject.Parse(resultResponseQR);
        var qrCodedto = new QRCodeDto
        {
            QRCode = jsonQR["qrCode"]?.ToString()
        };
        return qrCodedto;
    }

    private async Task<string?> GetToken()
    {
        var user = new
        {
            userName = "ibrahimia-1",
            password = "ibrahimiaP@ssw0rd"
        };
        var contentUser = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync("https://zatca.mazeedit.com/api/Auth/Login", contentUser);

        response.EnsureSuccessStatusCode();

        var resultResponse = await response.Content.ReadAsStringAsync();

        var json = JObject.Parse(resultResponse);
        var token = json["data"]?["token"]?.ToString();
        return token;
    }

    /// <inheritdoc/>
	public async Task<PagedResultDto<InvoiceDto>> GetAllInvoicesByOrganizationAsync(PagedAndSortedResultRequestDto input, Guid? tenantId)
    {
        var queryable = await _invoiceRepository.GetQueryableAsync();

        queryable = queryable.Where(t => t.TenantId == tenantId);

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var items = await AsyncExecuter.ToListAsync(
            queryable
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        // Map entities to DTOs
        var transactionDtos = ObjectMapper.Map<List<Invoice>, List<InvoiceDto>>(items);

        // Return paged result
        return new PagedResultDto<InvoiceDto>(totalCount, transactionDtos);
    }
}
