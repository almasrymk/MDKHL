using SmartParking.Entities.Customers;
using SmartParking.Enums;
using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartParking.Entities.Organizations;

public class CustomerPackage : FullAuditedEntity<int>, IBseEntity
{
    public virtual Customer Customer { get; set; }
    public Guid CustomerId { get; set; }
    public virtual Package? Package { get; set; }
    public int? PackageId { get; set; }
    public virtual Organization Organization { get; set; }
    public int? OrganizationId { get; set; }
    public CustomerPackageStatus CustomerPackageStatus { get; set; }
    public Guid? TenantId { get; set; }
    public Status Status { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int DurationOfPackage { get; set; }
}
