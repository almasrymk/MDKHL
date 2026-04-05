namespace SmartParking.CustomerPackages.Dtos;

public class CustomerPackageVehicleDto
{
    public string LicenseTypeImage { get; set; }
    public string VehicleModelImage { get; set; }
    public string EnPlateNumber { get; set; } = default!;
    public string ArPlateNumber { get; set; } = default!;
}
