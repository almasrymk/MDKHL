using SmartParking.Entities.Customers;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    //public int VehicleId { get; set; }
    //public virtual Vehicle Vehicle { get; set; }
    // public int PackageId { get; set; }
    // public virtual Package Package { get; set; }
    public Guid? TenantId { get; set; }
}
