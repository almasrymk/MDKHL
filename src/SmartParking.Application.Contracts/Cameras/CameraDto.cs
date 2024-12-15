using SmartParking.Entities.Lookups;
using SmartParking.Enums;
using SmartParking.Garages;
using SmartParking.Lookups.CameraBrands;
using SmartParking.Lookups.CameraMainType;
using SmartParking.Lookups.CameraModel;
using System;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Cameras;

public class CameraDto: AuditedEntityDto<int>
{
    public string Name { get; set; }
    public CameraModelDto Model { get; set; }
    public CameraMainTypeDto MainType { get; set; }
    public CameraBrandDto BrandName { get; set; }
    public string DeviceId { get; set; }
    public string CameraIP { get; set; }
    public string MacAddress { get; set; }
    public CameraType CameraType { get; set; }
    public int GarageId { get; set; }
    public Status Status { get; set; }
    public GarageDto Garage { get; set; }
    public Guid? TenantId { get; set; }
}
