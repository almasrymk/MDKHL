using SmartParking.Enums;
using SmartParking.Entities; // Assuming Invoice is here, wait I need the exact namespace
using SmartParking.Payments.PaymentIntegrations;
using SmartParking.Entities.Invoices;

namespace SmartParking.Payments;

public static class PaymentDtoExtensions
{
    public static PaymentCheckoutRequestDto ToPaymentCheckoutRequestDto(
        this Invoice invoice, 
        decimal amount, 
        PaymentMethod paymentMethod, 
        string? callbackUrl = null)
    {
        return new PaymentCheckoutRequestDto
        {
            InvoiceId = invoice.Id,
            OrganizationId = invoice.OrganizationId,
            Amount = amount,
            PaymentCategory = PaymentCategory.PaymentParking,
            PaymentMethod = paymentMethod,
            TenantId = invoice.TenantId.Value,
            CallbackUrl = callbackUrl,
        };
    }

    public static MobilePaymentSessionDto ToMobilePaymentSessionDto(this GatewayInitiateResult initiateResult)
    {
        return new MobilePaymentSessionDto
        {
            PaymentId = initiateResult.Gateway == GatewayKey.Geidea ? default : default,
            SessionId = initiateResult.GeideaSessionId ?? initiateResult.OppwaCheckoutId,
            SessionUrl = initiateResult.JsArguments.Length > 3 ? initiateResult.JsArguments[3]?.ToString() : null,
            MerchantReferenceId = initiateResult.PaymentNumber,
            ExpiresAt = null,
            GatewayResponse = initiateResult.GatewayResponseData
        };
    }

    public static PaymentCheckoutRequestDto ToPaymentCheckoutRequestDto(
        this SmartParking.Entities.Organizations.CustomerPackage customerPackage,
        decimal amount,
        PaymentMethod paymentMethod,
        string? callbackUrl = null)
    {
        return new PaymentCheckoutRequestDto
        {
            OrganizationId = customerPackage.OrganizationId ?? 0,
            PaymentCategory = PaymentCategory.Package,
            CustomerPackageId = customerPackage.Id,
            PackageId = customerPackage.PackageId,
            PaymentMethod = paymentMethod,
            Amount = amount,
            CustomerId = customerPackage.CustomerId,
            ArPlateNumber = customerPackage.ArPlateNumber,
            EnPlateNumber = customerPackage.EnPlateNumber,
            TenantId = customerPackage.TenantId ?? System.Guid.Empty,
            CallbackUrl = callbackUrl
        };
    }
}
