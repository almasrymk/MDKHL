using SmartParking.Entities.Invoices;
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
using Volo.Abp.ObjectMapping;

namespace SmartParking.Invoices;

public class InvoiceAppService : CrudAppService<Invoice,InvoiceDto,int, PagedAndSortedResultRequestDto,CreateUpdateInvoiceDto> , IInvoiceAppService
{
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<Invoice, int> _repository;

    public InvoiceAppService(IRepository<Invoice,int> repository, IDataFilter dataFilter) 
    :base(repository)
    {
        _dataFilter = dataFilter;
        _repository = repository;
    }

    public override async Task<InvoiceDto> CreateAsync(CreateUpdateInvoiceDto input)
    {
        if (CurrentUser.IsInRole("admin"))
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
        if (CurrentUser.IsInRole("admin"))
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
        if (CurrentUser.IsInRole("admin"))
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
                
                or.InvoiceType.ToString() == input.SearchingTerm);
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

}
