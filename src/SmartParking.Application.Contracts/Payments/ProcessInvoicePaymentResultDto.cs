using System;

namespace SmartParking.Payments;

public class ProcessInvoicePaymentResultDto
{
    public InvoicePaymentResult Result { get; set; }
    public Guid? PaymentId { get; set; }
    public string? PaymentNumber { get; set; }
    public PaymentCheckoutDto? CheckoutData { get; set; }
    public string? Message { get; set; }
}
