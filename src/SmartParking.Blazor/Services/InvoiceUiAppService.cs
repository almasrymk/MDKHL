using SmartParking.Blazor.Contracts;
using SmartParking.Blazor.Dtos;
using SmartParking.GarageTransactionEndPoints;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;

namespace SmartParking.Blazor.Services;

public class InvoiceUiAppService : ApplicationService, IInvoiceUiAppService
{
    private readonly IGarageTransactionActionsAppService _garageTransactionActionsAppService;
    private readonly IKeeperTransactionsAppService _keeperTransactionsAppService;
    public InvoiceUiAppService(IGarageTransactionActionsAppService garageTransactionActionsAppService
      ,
        IKeeperTransactionsAppService keeperTransactionsAppService)
    {
        _garageTransactionActionsAppService = garageTransactionActionsAppService;
        _keeperTransactionsAppService = keeperTransactionsAppService;
    }
    [RemoteService(false)]
    public async Task<GarageTransactionUiDto> GetGarageTransaction(InvoiceSearchDto invoiceSearchDto)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var dto = await _garageTransactionActionsAppService.GetGarageTransaction(invoiceSearchDto);
            var mapped = ObjectMapper.Map<GarageTransactionDto, GarageTransactionUiDto>(dto);
            return mapped;
        }

    }


    [RemoteService(false)]
    public async Task<CreateUpdateInvoiceUiDto> PreCheckout(GarageTransactionUiDto garageTransactionUiDto)
    {
        
        var input = ObjectMapper.Map<GarageTransactionUiDto, GarageTransactionDto>(garageTransactionUiDto);
        var dto = await _keeperTransactionsAppService.PreCheckout(input.Id);
        return ObjectMapper.Map<CreateUpdateInvoiceDto, CreateUpdateInvoiceUiDto>(dto.Value);
        
        
    }

    [RemoteService(false)]
    public async Task<CreateUpdateInvoiceUiDto> GetInvoiceAfterPreCheckout(InvoiceSearchDto invoiceSearchDto)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var transaction = await GetGarageTransaction(invoiceSearchDto);
            return await PreCheckout(transaction);
        }
    }

    [RemoteService(false)]
    public async Task<InvoiceUiDto> Checkout(GarageTransactionUiDto garageTransactionUiDto)
    {
        
        var input = ObjectMapper.Map<GarageTransactionUiDto, GarageTransactionDto>(garageTransactionUiDto);
        var dto = await _keeperTransactionsAppService.Checkout(input.Id);
        return ObjectMapper.Map<InvoiceDto, InvoiceUiDto>(dto);
        
    }

    [RemoteService(false)]
    public async Task<InvoiceUiDto> GetInvoiceAfterCheckout(InvoiceSearchDto invoiceSearchDto)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var transaction = await GetGarageTransaction(invoiceSearchDto);
            return await Checkout(transaction);
        }
    }
}
