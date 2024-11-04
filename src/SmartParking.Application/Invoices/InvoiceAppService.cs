using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Enums;
using SmartParking.Organizations;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;

namespace SmartParking.Invoices;
//[Authorize]
public class InvoiceAppService : CrudAppService<Invoice,InvoiceDto,int, PagedAndSortedResultRequestDto,CreateUpdateInvoiceDto> , IInvoiceAppService
{
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<Invoice, int> _repository;
    private readonly IOrganizationsAppService _organizationsAppService;
    private readonly HttpClient _httpClient;

    public InvoiceAppService(IRepository<Invoice,int> repository, IDataFilter dataFilter,IOrganizationsAppService organizationsAppService, HttpClient httpClient) 
    :base(repository)
    {
        _dataFilter = dataFilter;
        _repository = repository;
        _organizationsAppService = organizationsAppService;
        _httpClient = httpClient;
    }

    public override async Task<InvoiceDto> CreateAsync(CreateUpdateInvoiceDto input)
    {
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
        if (CurrentUser.IsInRole(RolesType.Mdkhl.ToString()) || CurrentUser.IsInRole("admin"))
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
        var queryable = await _repository.GetQueryableAsync();
      
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
        var queryable = await _repository.GetQueryableAsync();
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
        var invoice = await _repository.FindAsync(id);

        if (invoice is null)
        {
            throw new EntityNotFoundException(typeof(VehicleModel), id);
        }


        await base.DeleteAsync(id);
    }

    [RemoteService(IsEnabled =false)]

	public async Task<string> GenerateInvoiceNumberAsync(int organizationId)
	{
		var organization = await _organizationsAppService.GetAsync(organizationId);
		var currentYearSuffix = DateTime.Now.Year % 100;  // Last 2 digits of the year
		var prefix = organization.EnglishName.Substring(0, 3).ToUpper();

        // Fetch the last invoice for the current year and organization
        var invoices = await _repository.GetListAsync(i => i.OrganizationId == organizationId);
			
        var lastInvoice = invoices
        .Where(i => i.CreationTime.Year == DateTime.Now.Year)
		.OrderByDescending(i => i.InvoiceNumber)
		.FirstOrDefault();

        
		int nextInvoiceSequence = 1;

		if (lastInvoice != null)
		{
            // Extract the last 3 digits of the previous invoice number (e.g., "ABC23-001")
            var lastSequenceString = lastInvoice.InvoiceNumber[^3..];
			if (int.TryParse(lastSequenceString, out int lastSequence))
			{
				nextInvoiceSequence = lastSequence + 1;
			}
		}

		// Format the sequence as a three-digit number with leading zeros
		var formattedSequence = nextInvoiceSequence.ToString().PadLeft(3,'0') ;

		// Combine all parts to form the final invoice number
		return $"{prefix}{currentYearSuffix}{formattedSequence}";
	}

    public async Task<Result<QRCodeDto>> GetInvoiceQRCode(int invoiceId)
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
        var invoice = await _repository.GetAsync(invoiceId);
        //var organization = await _organizationsAppService.GetAsync(invoice.OrganizationId);
        if (invoice is null)
        {
            return Result.NotFound();
        }
        decimal percentage = invoice.InvoicePrice * 0.14m;
        var qR = new
        {
            sellerName = invoice.Organization.EnglishName,
            vatRegistration = invoice.Organization.UnifiedLicenseNum,
            timeStamp = DateTime.Now.ToString(),
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
        return Result<QRCodeDto>.Success(qrCodedto);
    }
}
