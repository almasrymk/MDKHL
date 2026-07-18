using Microsoft.Extensions.Logging;
using SmartParking.Entities.Invoices;
using SmartParking.Enums;
using SmartParking.Results;
using SmartParking.Utilities;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Payments.Strategies;

public class InvoicePaymentInitializationStrategy : IPaymentInitializationStrategy, ITransientDependency
{
    private readonly IRepository<Invoice, int> _invoiceRepo;
    private readonly ILogger<InvoicePaymentInitializationStrategy> _logger;

    public InvoicePaymentInitializationStrategy(
        IRepository<Invoice, int> invoiceRepo,
        ILogger<InvoicePaymentInitializationStrategy> logger)
    {
        _invoiceRepo = invoiceRepo;
        _logger = logger;
    }

    public PaymentCategory Category => PaymentCategory.PaymentParking;

    public async Task<Result<PaymentInitializationContext>> BuildAsync(int sourceId, PaymentMethod paymentMethod)
    {
        var invoiceResult = await GetInvoiceByIdAsync(sourceId);
        if (!invoiceResult.IsSuccess)
        {
            _logger.LogWarning("InvoicePaymentInitializationStrategy: {Error}", invoiceResult.Errors.FirstOrDefault());
            return Result<PaymentInitializationContext>.Error(invoiceResult.Errors.FirstOrDefault() ?? "Error initializing invoice payment");
        }

        var invoice = invoiceResult.Value;
        var amount = PaymentUtilityCalculationts.GetPriceForPayment(invoice.InvoicePrice, invoice.Discount);

        var request = invoice.ToPaymentCheckoutRequestDto(amount, paymentMethod);

        return Result<PaymentInitializationContext>.Success(new PaymentInitializationContext
        {
            Amount = amount,
            CheckoutRequest = request
        });
    }

    private async Task<Result<Invoice>> GetInvoiceByIdAsync(int invoiceId)
    {
        var invoice = await _invoiceRepo.FirstOrDefaultAsync(inv => inv.Id == invoiceId);
        if (invoice == null)
        {
            return Result<Invoice>.Error("Invoice not found");
        }
        if (invoice.InvoiceType == InvoiceType.Paid || invoice.InvoicePrice == 0)
        {
            return Result<Invoice>.Error("Invoice already paid");
        }
        return Result<Invoice>.Success(invoice);
    }
}
