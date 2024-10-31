using Microsoft.AspNetCore.Authorization;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Enums;
using SmartParking.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
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
public class InvoiceAppService : CrudAppService<Invoice,InvoiceDto,int, PagedAndSortedResultRequestDto,CreateUpdateInvoiceDto> , IInvoiceAppService
{
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<Invoice, int> _repository;
    private readonly IOrganizationsAppService _organizationsAppService;
    public InvoiceAppService(IRepository<Invoice,int> repository, IDataFilter dataFilter,IOrganizationsAppService organizationsAppService) 
    :base(repository)
    {
        _dataFilter = dataFilter;
        _repository = repository;
        _organizationsAppService = organizationsAppService;
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
}
