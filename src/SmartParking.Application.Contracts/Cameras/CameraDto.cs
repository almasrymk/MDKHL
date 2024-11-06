using SmartParking.Enums;
using SmartParking.Garages;
using System;

namespace SmartParking.Cameras;

public class CameraDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Model { get; set; }
    public string MainType { get; set; }
    public string BrandName { get; set; }
    public string DeviceId { get; set; }
    public string CameraIP { get; set; }
    public string MacAddress { get; set; }
    public CameraType CameraType { get; set; }
    public int GarageId { get; set; }
    public Status Status { get; set; }
    public GarageDto Garage { get; set; }
    public Guid? TenantId { get; set; }
}
