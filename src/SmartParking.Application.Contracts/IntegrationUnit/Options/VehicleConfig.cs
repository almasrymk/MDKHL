namespace SmartParking.IntegrationUnit.Options;

public class VehicleConfig
{
    public string BrandName { get; set; }
    public string ModelName { get; set; }
    public string PlateColorName { get; set; }
    public string LicenceTypeName { get; set; }
}

public class MyIntegrationOptions
{
    public VehicleConfig Vehicle { get; set; }
}
