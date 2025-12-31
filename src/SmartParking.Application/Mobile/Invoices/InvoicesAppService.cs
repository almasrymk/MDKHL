using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Payments;
using SmartParking.Entities.Vehicles;
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
    private readonly IRepository<Payment, Guid> _paymentRepoistory;
    private readonly IRepository<Vehicle, int> _vehicleRepository;
    private readonly IDataFilter _dataFilter;

    public InvoicesAppService(IRepository<Invoice, int> invoiceRepoistory, 
        IDataFilter dataFilter, 
        IRepository<Payment, Guid> paymentRepoistory,
        IRepository<Vehicle, int> VehicleRepository)
    {
        _invoiceRepoistory = invoiceRepoistory;
        _dataFilter = dataFilter;
        _paymentRepoistory = paymentRepoistory;
        _vehicleRepository = VehicleRepository;
    }
    /// <inheritdoc/>

    public async Task<Result<InvoiceDto>> CreateAsync(CreateUpdateInvoiceDto input)
    {
        try
        {
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
            var payment = await _paymentRepoistory.FirstOrDefaultAsync( x => x.InvoiceId == id);
            var vehicle = await _vehicleRepository.FirstOrDefaultAsync(x => x.CustomerId == invoice.CustomerId);
            if (invoice == null)
                return Result<InvoiceDto>.NotFound("There is no such an Invoice , id: 5");
            var invoiceMap = ObjectMapper.Map<Invoice, InvoiceDto>(invoice);
            invoiceMap.QrCodeInvoice =  payment is null  ? "" : payment.QrCodeInvoice;
            invoiceMap.LicenseTypeImage = vehicle is null ? "" : vehicle.LicenceType.Image;
            invoiceMap.VehicleModelImage = vehicle is null ? "" : vehicle.VehicleModel.Image;
            return Result<InvoiceDto>.Success(invoiceMap);
        }
       
    }
    /// <inheritdoc/>

    public async Task<Result<List<InvoiceDto>>> GetListAsync()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryable = await _invoiceRepoistory.GetQueryableAsync();
            var invoices = await queryable.Where(_ => _.Customer.IdentityUserId == CurrentUser.Id)
                .OrderByDescending(_ => _.CreationTime)
                .ToListAsync();
            return ObjectMapper.Map<List<Invoice>, List<InvoiceDto>>(invoices);
        }
        
    }
}
