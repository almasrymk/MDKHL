using SmartParking.Enums;
using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.UnitVehiclePackges;

public class UnitVehiclePackge : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid CustomerId { get; set; }
    public string UnitCode { get; set; }
    public DateTime ActivationDate { get; set; }
    public DateTime DectivationDate { get; set; }
    public TimeOnly PublishFrom { get; set; }
    public TimeOnly PublishTo { get; set; }
    public string EnPlateNumber { get; set; }
    public string ArPlateNumber { get; set; }
    public int CustomerUnitPackageId { get; set; }
    public CustomerUnitPackageType CustomerUnitPackageType { get; set; }
    public string PackageCode { get; set; }

    public Guid? TenantId {  get; set; }
}
