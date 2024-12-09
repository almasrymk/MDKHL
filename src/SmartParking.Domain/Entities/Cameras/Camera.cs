using SmartParking.Entities.Lookups;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Cameras;

public class Camera : FullAuditedEntity<int>, IMultiTenant, IBseEntity
{
    public string Name { get; set; }
    public int ModelId { get; set; }
    public int MainTypeId { get; set; }
    public int BrandNameId { get; set; }
    public virtual CameraModel Model { get; set; }
    public virtual CameraMainType MainType { get; set; }
    public virtual CameraBrand BrandName { get; set; }
    public string DeviceId { get; set; }
    public string CameraIP { get; set; }
    public string MacAddress { get; set; }
    public CameraType CameraType { get; set; }
    public virtual Garage Garage { get; set; }
    public virtual Organization Organization { get; set; }
    public int GarageId { get; set; }
    public int OrganizationId { get; set; }
    public Status Status { get; set; }
    public Guid? TenantId { get; set; }
}
