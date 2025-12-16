
using Abp.Application.Services;
using SmartParking.GarageTransactions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartParking.Invoices.Factory;

public class InvoiceActionsFactoryAppService : IInvoiceActionsFactoryAppService
{
    private readonly IEnumerable<IInvoiceActionsAppService> _services;
    #region public methods
    public bool CanHandle(InvoiceSearchDto invoiceSearchDto)
    {
        throw new System.NotImplementedException();
    }

    public Task<InvoiceDto> Search(InvoiceSearchDto invoiceSearchDto)
    {
        throw new System.NotImplementedException();
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
