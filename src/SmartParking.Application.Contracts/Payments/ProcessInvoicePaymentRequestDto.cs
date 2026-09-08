using SmartParking.Enums;

namespace SmartParking.Payments;

public class ProcessInvoicePaymentRequestDto
{
    public int InvoiceId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal? Discount { get; set; }
    public string? TerminalId { get; set; }
}
