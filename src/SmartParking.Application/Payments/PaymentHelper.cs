using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Payments;
using SmartParking.Enums;
using SmartParking.GarageTransactions.Factory;
using SmartParking.Invoices;
using SmartParking.Results;
using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Payments;

public class PaymentMobileHelper : ApplicationService, IPaymentMobileHelper
{
    private readonly PublicInvoiceAppService _publicInvoiceAppService;


    public PaymentMobileHelper(PublicInvoiceAppService publicInvoiceAppService)
    {
        _publicInvoiceAppService = publicInvoiceAppService;

    }

    public async Task<Result> HandleCancel(int invoiceId)
    {
        
        try
        {
            await _publicInvoiceAppService.CancelInvoiceAsync(invoiceId);
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
        return Result.Success();
    }

    
}
