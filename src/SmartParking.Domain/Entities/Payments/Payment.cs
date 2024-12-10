using SmartParking.Entities.Customers;
using SmartParking.Enums;
using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartParking.Entities.Payments;

public class Payment : FullAuditedEntity<Guid>
{
    public int OrganizationId { get; set; }
    public string? EntityId { get; set; }
    public int? InvoiceId { get; set; }
    public int? PackageId { get; set; }
    public string? CheckoutId { get; set; }
    public Guid? CustomerId { get; set; }

    public PaymentCategory PaymentCategory { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public int PaymentCategoryId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string Currency { get; set; }
    public virtual Customer Customers { get; set; }
    public string QrCodeInvoice { get; set; }

    public string? PaymentName { get; set; }

}
