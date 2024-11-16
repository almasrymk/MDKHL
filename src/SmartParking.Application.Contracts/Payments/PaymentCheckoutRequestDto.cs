using SmartParking.Enums;

namespace SmartParking.Payments;

public class PaymentCheckoutRequestDto
{
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentCategory PaymentCategory { get; set; }
    public string? InvoiceId { get; set; }
    public string? PackageId { get; set; }
    public decimal Amount { get; set; }
    public int OrganizationId { get; set; }
}
