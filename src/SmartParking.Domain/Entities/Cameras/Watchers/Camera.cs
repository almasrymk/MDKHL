using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Cameras.Watchers;

public class Camera : Watcher
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public int ModelId { get; set; }
    public int MainTypeId { get; set; }
    public int BrandNameId { get; set; }
    public virtual Lookups.CameraModel Model { get; set; }
    public virtual Lookups.CameraMainType MainType { get; set; }
    public virtual Lookups.CameraBrand BrandName { get; set; }
    public string CameraIP { get; set; }
    public string MacAddress { get; set; }
    public virtual Garage Garage { get; set; }
    public virtual Organization Organization { get; set; }
}
