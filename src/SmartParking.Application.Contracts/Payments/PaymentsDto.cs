using SmartParking.Enums;
using System;

namespace SmartParking.Payments;

public class PaymentDto
{

    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CheckoutId { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public int? InvoiceId { get; set; }
    public int? PackageId { get; set; }
    public PaymentCategory PaymentCategory { get; set; }
    public int OrganizatioId { get; set; }
    public string QrCodeInvoice { get; set; }


}
