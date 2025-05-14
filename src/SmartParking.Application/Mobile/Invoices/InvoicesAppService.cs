using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Invoices;
using SmartParking.Enums;
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
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Mobile.Invoices;

[RemoteService(IsEnabled = false)]
public class InvoicesAppService : ApplicationService, IInvoicesAppService
{
    private readonly IRepository<Invoice, int> _invoiceRepoistory;
    private readonly IDataFilter _dataFilter;

    public InvoicesAppService(IRepository<Invoice, int> invoiceRepoistory,IDataFilter dataFilter)
    {
        _invoiceRepoistory = invoiceRepoistory;
        _dataFilter = dataFilter;
    }
    /// <inheritdoc/>

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
    /// <inheritdoc/>

    public async Task<Result<InvoiceDto>> GetAsync(int id)
    {
        using(_dataFilter.Disable<IMultiTenant>()) 
        {
            var invoice = await _invoiceRepoistory.FindAsync(id);
            if (invoice == null)
                return Result<InvoiceDto>.NotFound("There is no such an Invoice , id: 5");
            var invoiceMap = ObjectMapper.Map<Invoice, InvoiceDto>(invoice);
            return Result<InvoiceDto>.Success(invoiceMap);
        }
       
    }
    /// <inheritdoc/>

    public async Task<Result<List<InvoiceDto>>> GetListAsync()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryable = await _invoiceRepoistory.GetQueryableAsync();
            //queryable = queryable.Include(c => c.Customer.IdentityUser);

            queryable = queryable.Where(_ => _.Customer.IdentityUserId == CurrentUser.Id);


            // Execute the query and get the list of hotel entities
            var invoices = await AsyncExecuter.ToListAsync(queryable);

            // Map the hotel entities to hotelDto objects
            var invoicesDto = ObjectMapper.Map<List<Invoice>, List<InvoiceDto>>(invoices);
            // Return the paginated result
            return invoicesDto;
        }
        
    }
}
