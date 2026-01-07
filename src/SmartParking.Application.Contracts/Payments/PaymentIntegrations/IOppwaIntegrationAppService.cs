using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Payments.PaymentIntegrations;

public interface IOppwaIntegrationAppService
{
    Task<string> PrepareCheckoutAsync(
        decimal amount,
           PaymentMethod brand );
    Task SavePaymentRequest(PaymentCheckoutRequestDto paymentCheckoutRequest,
       string checkoutid );

    Task<PaymentVerificationResult> VerifyHyperPayPaymentAsync(string resourcePath);
}
