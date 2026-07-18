using SmartParking.Entities.Invoices;
using SmartParking.Entities.Payments;
using SmartParking.Enums;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Payments.Strategies;

public class InvoicePaymentCompletionStrategy : IPaymentCompletionStrategy, ITransientDependency
{
    private readonly IRepository<Invoice, int> _invoiceRepo;

    public InvoicePaymentCompletionStrategy(IRepository<Invoice, int> invoiceRepo)
    {
        _invoiceRepo = invoiceRepo;
    }

    public PaymentCategory Category => PaymentCategory.PaymentParking;

    public async Task CompleteAsync(Payment payment)
    {
        var invoice = await _invoiceRepo.FirstOrDefaultAsync(i => i.Id == payment.InvoiceId);
        if (invoice != null)
        {
            invoice.InvoiceType = InvoiceType.Paid;
            invoice.InvoiceNumber = payment.PaymentNumber;
            await _invoiceRepo.UpdateAsync(invoice);
        }
    }
}
