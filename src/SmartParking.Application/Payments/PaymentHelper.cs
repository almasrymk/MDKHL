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

    public async Task<Result> HandleCancel(int invoiceId, string input)
    {
        var invoiceSearchDto = new InvoiceSearchDto();

        if (!string.IsNullOrEmpty(input) && char.IsLetter(input.LastOrDefault()))
        {
            invoiceSearchDto.TicketId = string.Empty;
            invoiceSearchDto.PlateNumber = input;
        }
        else
        {
            invoiceSearchDto.TicketId = input ?? string.Empty;
            invoiceSearchDto.PlateNumber = string.Empty;
        }
        try
        {
            await _publicInvoiceAppService.CancelInvoiceAsync(invoiceId,invoiceSearchDto);
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
        return Result.Success();
    }

    
}
