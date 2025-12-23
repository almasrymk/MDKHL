
using Abp.Application.Services;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.GarageTransactions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartParking.Invoices.Factory;

public class InvoiceActionsFactoryAppService : IInvoiceActionsFactoryAppService
{
    private readonly IEnumerable<IInvoiceActionsAppService> _services;

    public InvoiceActionsFactoryAppService(IEnumerable<IInvoiceActionsAppService> services)
    {
        _services = services;
    }

    #region public methods

    public async Task<InvoiceDto> Search(InvoiceSearchDto invoiceSearchDto)
    {
        var service = GetConsumerService(invoiceSearchDto);
        return await service.Search(invoiceSearchDto);
    }

    public async Task<Watcher> GetWatcherByCodeAsync(InvoiceSearchDto invoiceSearchDto, string watcherCode)
    {
        var service = GetConsumerService(invoiceSearchDto);
        var watcher = await service.GetWatcherByCodeAsync(watcherCode);
        return watcher;
    }
    #endregion
    #region private methods
    private IInvoiceActionsAppService GetConsumerService(InvoiceSearchDto invoiceSearchDto)
    {
        foreach (var service in _services)
        {
            if (service.CanHandle(invoiceSearchDto))
            {
                return service;
            }
        }
        throw new Exception("Invalid Action!!");
    }
    #endregion
}
