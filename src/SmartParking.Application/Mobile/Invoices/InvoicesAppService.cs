using SmartParking.Entities.Invoices;
using SmartParking.Invoices;
using SmartParking.MobileInvoices;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Mobile.Invoices;

[RemoteService(IsEnabled = false)]
public class InvoicesAppService : ApplicationService, IInvoicesAppService
{
    private readonly IRepository<Invoice, int> _invoiceRepoistory;
    public InvoicesAppService(IRepository<Invoice, int> invoiceRepoistory)
    {
        _invoiceRepoistory = invoiceRepoistory;
    }
    public async Task<Result<InvoiceDto>> CreateAsync(CreateUpdateInvoiceDto input)
    {
        try
        {
            // Check if the user is authenticated
            //if (!_currentUser.IsInRole("admin"))
            //{
            //    return Result<VehicleDto>.Error("User is not authenticated");
            //}
            var mappedInvoice = ObjectMapper.Map<CreateUpdateInvoiceDto, Invoice>(input);

            var invoice = await _invoiceRepoistory.InsertAsync(mappedInvoice, autoSave: true);

            var invoiceDto = ObjectMapper.Map<Invoice, InvoiceDto>(invoice);

            return Result<InvoiceDto>.Success(invoiceDto, "Invoice Add Succesuffuly");
        }
        catch (Exception ex)
        {
            return Result<InvoiceDto>.Error(ex.Message);
        }
    }

    public async Task<Result<PagedResultDto<InvoiceDto>>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _invoiceRepoistory.GetQueryableAsync();

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
        var invoicesDto = ObjectMapper.Map<List<Invoice>, List<InvoiceDto>>(invoices);
        // Return the paginated result
        return new Result<PagedResultDto<InvoiceDto>>(
           new PagedResultDto<InvoiceDto>
           (totalCount, invoicesDto)
       );
    }
}
