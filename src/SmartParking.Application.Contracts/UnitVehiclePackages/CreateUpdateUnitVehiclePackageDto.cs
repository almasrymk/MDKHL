using SmartParking.Enums;
using System;

namespace SmartParking.UnitVehiclePackages;

public class CreateUpdateUnitVehiclePackageDto
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


}
