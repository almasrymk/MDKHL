using SmartParking.Enums;
using System;

namespace SmartParking.Payments;

public class PaymentCheckoutRequestDto
{
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentCategory PaymentCategory { get; set; }
    public int? InvoiceId { get; set; }
    public int? PackageId { get; set; }
    public decimal Amount { get; set; }
    public int OrganizationId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? PaymentName { get; set; }

}
