using SmartParking.Entities.Payments;
using SmartParking.Enums;
using SmartParking.Organizations;
using SmartParking.Invoices;
using SmartParking.Results;
using SmartParking.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Payments;

public class PaymentCompletionService : IPaymentCompletionService, ITransientDependency
{
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IEnumerable<IPaymentCompletionStrategy> _strategies;
    private readonly IDataFilter _dataFilter;
    private readonly IZatcaTaxInvoiceAppService _zatcaTaxInvoiceAppService;
    private readonly IOrganizationsAppService _organizationsAppService;
    private readonly IInvoiceUtilityAppService _invoiceUtility;

    public PaymentCompletionService(
        IRepository<Payment, Guid> paymentRepository,
        IEnumerable<IPaymentCompletionStrategy> strategies,
        IDataFilter dataFilter,
        IZatcaTaxInvoiceAppService zatcaTaxInvoiceAppService,
        IOrganizationsAppService organizationsAppService,
        IInvoiceUtilityAppService invoiceUtility)
    {
        _paymentRepository = paymentRepository;
        _strategies = strategies;
        _dataFilter = dataFilter;
        _zatcaTaxInvoiceAppService = zatcaTaxInvoiceAppService;
        _organizationsAppService = organizationsAppService;
        _invoiceUtility = invoiceUtility;
    }

    public async Task<Guid> CompletePaymentByCheckoutIdAsync(string checkoutId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var payment = await _paymentRepository.FirstOrDefaultAsync(p => p.CheckoutId == checkoutId);
            if (payment == null)
            {
                throw new Exception("Payment not found.");
            }

            return await ProcessPaymentCompletionAsync(payment);
        }
    }

    public async Task<Guid> CompletePaymentByPaymentNumberAsync(string paymentNumber, string checkoutId, PaymentMethod paymentMethod)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var payment = await _paymentRepository.FirstOrDefaultAsync(p => p.PaymentNumber == paymentNumber);
            if (payment == null)
            {
                throw new Exception("Payment not found.");
            }

            payment.CheckoutId = checkoutId;
            payment.PaymentMethod = paymentMethod;

            return await ProcessPaymentCompletionAsync(payment);
        }
    }

    private async Task<Guid> ProcessPaymentCompletionAsync(Payment payment)
    {
        var strategy = _strategies.FirstOrDefault(s => s.Category == payment.PaymentCategory);
        if (strategy != null)
        {
            await strategy.CompleteAsync(payment);
        }

        payment.PaymentStatus = PaymentStatus.Success;

        await CallingZatcaIntegration(payment);

        await _paymentRepository.UpdateAsync(payment);

        return payment.Id;
    }

    private async Task CallingZatcaIntegration(Payment payment)
    {
        QRCodeDto qrCode = new QRCodeDto();
        var organization = await _organizationsAppService.GetAsync(payment.OrganizationId);
        if (organization != null && organization.StatusZatca == Status.Active && payment.Amount > 0)
        {
            var res = await _zatcaTaxInvoiceAppService.SendToZACTA(payment.Id);
            payment.QrCodeInvoice = qrCode.QRCode = res.invoiceQRCode;
            payment.IsSentZatca = res.status;
            payment.ErrorMessagesZatca = null;
            if (res.validationResults != null && res.validationResults.errorMessages != null && res.validationResults.errorMessages.Count > 0)
            {
                var listErrors = res.validationResults.errorMessages.Select(e => e.Message).ToList();
                payment.ErrorMessagesZatca = string.Join(",", listErrors);
            }
            else if (res.validationResults != null && res.validationResults.warningMessages != null && res.validationResults.warningMessages.Count > 0)
            {
                var listErrors = res.validationResults.warningMessages.Select(e => e.Message).ToList();
                payment.ErrorMessagesZatca = string.Join(",", listErrors);
            }
        }
        else
        {
            qrCode = await _invoiceUtility.GetQrCodeAsync(payment.Amount, payment.OrganizationId);
            payment.QrCodeInvoice = qrCode.QRCode;
        }
    }
}
