using System;
using Volo.Abp.Application.Dtos;

namespace SmartParking.UnitPackageIntegrations.Dtos;

public class UnitPackageIntegrationDto : AuditedEntityDto<Guid>
{
    public string BookingId { get; set; }
    public Guid UnitVehiclePackageId { get; set; }
    public string UnitCode { get; set; }
    public int OrganizationId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? TenantId { get; set; }
}
