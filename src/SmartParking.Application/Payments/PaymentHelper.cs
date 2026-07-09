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
    private readonly IInvoiceAppService _invoiceAppService;
    private readonly PaymentWebAppService _paymentWebAppService;
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IRepository<GarageTransaction, int> _garageTransactionRepository;
    private readonly IGarageTransactionsActionsFactoryAppService _garageTransactionsActionsFactoryAppService;

    public PaymentMobileHelper(PublicInvoiceAppService publicInvoiceAppService,
        IInvoiceAppService invoiceAppService,
        PaymentWebAppService paymentWebAppService,
        IRepository<GarageTransaction, int> garageTransactionRepository,
        IGarageTransactionsActionsFactoryAppService garageTransactionsActionsFactoryAppService)
    {
        _publicInvoiceAppService = publicInvoiceAppService;
        _invoiceAppService = invoiceAppService;
        _paymentWebAppService = paymentWebAppService;
        _garageTransactionRepository = garageTransactionRepository;
        _garageTransactionsActionsFactoryAppService = garageTransactionsActionsFactoryAppService;
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

    private async Task CancelInvoiceAsync(int invoiceId, InvoiceSearchDto searchDto)
    {
        var invoice = await _invoiceAppService.GetAsync(invoiceId);

        if (invoice == null)
            throw new UserFriendlyException("Invoice not found.");

        if (invoice.InvoiceType == InvoiceType.Paid.ToString())
            throw new UserFriendlyException("Invoice is already paid.");

        var exitTransactionId = invoice.GarageTransactionExitId;

        // Delete payment
        var payment = await _paymentWebAppService.GetPaymentForInvoice(invoiceId);
        if (payment != null)
        {
            await _paymentRepository.DeleteAsync(payment.Id);
        }

        await _invoiceAppService.DeleteAsync(invoice.Id);
        // Delete exit transaction
        if (exitTransactionId > 0)
        {
            await _garageTransactionRepository.DeleteAsync(exitTransactionId);
        }

        // Restore entry transaction
        var entryTransaction =
            await _garageTransactionsActionsFactoryAppService.GetGarageTransaction(searchDto);
        await _garageTransactionsActionsFactoryAppService.UnMappingEntryTransactionForDeletedExit(entryTransaction);


    }
}
