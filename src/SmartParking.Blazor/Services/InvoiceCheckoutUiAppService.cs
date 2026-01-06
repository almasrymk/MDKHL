using Volo.Abp.Domain.Repositories;
using SmartParking.Blazor.Contracts;
using SmartParking.Blazor.Dtos;
using SmartParking.Entities.Payments;
using SmartParking.Payments;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Blazor.Services;

public class InvoiceCheckoutUiAppService : ApplicationService, IInvoiceCheckoutUiAppService
{
    private readonly IRepository<Payment,Guid> _paymentRepository;
    
    public InvoiceCheckoutUiAppService(IRepository<Payment, Guid> paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }
    [RemoteService(false)]
    public async Task<InvoiceCheckoutUiDto> GetInvoiceCheckoutAsync(Guid paymentId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {

        //InvoiceCheckoutUiDto invoiceCheckout = null;
            var payment = await _paymentRepository.GetAsync(paymentId);
            var invoiceCheckout = new InvoiceCheckoutUiDto
            {
                PaymentNumber = payment.PaymentNumber,
                InvoiceId = payment.InvoiceId,
                InvoiceNumber = payment.Invoice.InvoiceNumber,
                EntranceTime = payment.Invoice.EntranceTime,
                InvoiceTime = payment.Invoice.InvoiceTime,
                PlateNumber = payment.ArPlateNumber,
                InvoicePrice = payment.Amount,
                Discount = payment.Invoice.Discount,
                IsPackage = payment.Invoice.IsPackage,
                BranchName = payment.Invoice.Organization.ArabicName,
                OrganizationAddress = payment.Invoice.Organization.Address,
                QrCodeInvoice = payment.QrCodeInvoice,
                CashierName = "System",
                Vat = (double)((payment.Invoice.InvoicePrice - payment.Invoice.Discount) * (decimal)0.15),
                OrganizationPicture = payment.Invoice.Organization.OrganizationPicture
            };
            return invoiceCheckout;
        }
    }
}
