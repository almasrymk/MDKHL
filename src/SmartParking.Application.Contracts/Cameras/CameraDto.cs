using SmartParking.Cameras.Watchers;
using SmartParking.Entities.Lookups;
using SmartParking.Enums;
using SmartParking.Garages;
using SmartParking.Lookups.CameraBrands;
using SmartParking.Lookups.CameraMainType;
using SmartParking.Lookups.CameraModel;
using System;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Cameras;

public class CameraDto: WatcherDto
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public CameraModelDto Model { get; set; }
    public CameraMainTypeDto MainType { get; set; }
    public CameraBrandDto BrandName { get; set; }
    public string CameraIP { get; set; }
    public string MacAddress { get; set; }
    public string GarageNameAr { get; set; }
    public string GarageNameEn { get; set; }
    public GarageDto Garage { get; set; }
}
