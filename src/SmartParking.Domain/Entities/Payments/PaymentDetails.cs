using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Payments;

public class PaymentDetails : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid PaymentId { get; set; }
    public int PayableId { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public decimal Vat { get; set; }
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }

    public Guid? TenantId { get; set; }
}
