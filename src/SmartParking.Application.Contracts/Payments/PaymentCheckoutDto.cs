using SmartParking.Enums;
using System;

namespace SmartParking.Payments;

public class PaymentCheckoutDto
{
    public string CheckoutId { get; set; }
    public string EntityId { get; set; }
    public string TokenId { get; set; }
    public Guid PaymentId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string QrCodeInvoice { get; set; }


}
