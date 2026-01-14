using SmartParking.Entities.Payments;
using SmartParking.Enums;
using SmartParking.Results;
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
           PaymentMethod brand ,
           PaymentCheckoutRequestDto paymentCheckoutRequest);
    Task SavePaymentRequest(PaymentCheckoutRequestDto paymentCheckoutRequest,
       string checkoutid );

    Task<PaymentVerificationResult> VerifyHyperPayPaymentAsync(string resourcePath);

    Task<string> InitiatePaymentAsync(PaymentCheckoutRequestDto paymentCheckoutRequest, string checkoutid = null);
    Task<Result> HandlePayment(Guid? paymentId);
}
