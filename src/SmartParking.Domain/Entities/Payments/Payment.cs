using SmartParking.Entities.Customers;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Payments;

public class Payment: FullAuditedEntity<Guid>
{
    public string? CheckoutId { get; set; }

    public PaymentCategory PaymentCategory { get; set; }
    public int PaymentCategoryId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string PaymentType { get; set; }
    public string Currency { get; set; }
    public string EntityId { get; set; }
    public Guid CustomerId { get; set; }
    public virtual Customer Customers { get; set; }

}
