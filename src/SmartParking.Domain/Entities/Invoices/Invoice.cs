using SmartParking.Entities.Customers;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Invoices;

public class Invoice : FullAuditedEntity<int> , IMultiTenant
{
    public string InvoiceNumber { get; set; }
    public int TotalHours { get; set; }
    public decimal InvoicePrice { get; set; }
    public InvoiceType InvoiceType { get; set; }
    public int OrganizationId { get; set; }
    public virtual Organization Organization { get; set; }
    public int GarageId  { get; set; }
    public virtual Garage Garage { get; set; }
	public string PlateNumber { get; set; }
    public int GarageTransactionExitId { get; set; }
    public Guid? CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }
    public DateTime? InvoiceTime { get; set; }
    public DateTime? EntranceTime  { get; set; }
    public Guid? TenantId { get; set; }
    public string CustomerArName { get; set; }
    public string CustomerEnName { get; set; }
    public bool IsPackage { get; set; }


}
