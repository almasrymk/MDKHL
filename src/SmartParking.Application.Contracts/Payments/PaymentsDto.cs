using SmartParking.Cutomers;
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
    public int OrganizationId { get; set; }
    public string QrCodeInvoice { get; set; }
    public CustomerDto Customers { get; set; }
    public string PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string CreationTime { get; set; }
    public string PaymentNumber { get; set; }
    public string PlateNumber { get; set; }
    public string OrganizationArabicName { get; set; }
    public string OrganizationEnglishName { get; set; }





}
