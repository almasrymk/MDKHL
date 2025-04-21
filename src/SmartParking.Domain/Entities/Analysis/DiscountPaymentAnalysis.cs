using SmartParking.Entities.Payments;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Analysis;

public class DiscountPaymentAnalysis
{
    public string PaymentNumber { get; set; }
    public int? InvoiceId { get; set; }
    public string CustomerArabicName { get; set; }
    public string CustomerEnglishName { get; set; }
    public Guid? TenantId { get; set; }
    public string? PlateNumber { get; set; }
    public string? ArPlateNumber { get; set; }
    public string Currency { get; set; }
    public PaymentCategory PaymentCategory { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public decimal? Discount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
}
