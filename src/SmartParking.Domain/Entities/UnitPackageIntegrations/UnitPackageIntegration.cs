using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.UnitPackageIntegrations;

public class UnitPackageIntegration : FullAuditedEntity<Guid>, IMultiTenant
{
    public string BookingId { get; set; }
    public Guid UnitVehiclePackageId { get; set; }
    public string UnitCode { get; set; }
    public int OrganizationId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? TenantId { get; set; }

}
