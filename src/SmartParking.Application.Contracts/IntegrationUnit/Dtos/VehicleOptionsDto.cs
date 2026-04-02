using System;

namespace SmartParking.IntegrationUnit.Dtos;

public class VehicleOptionsDto
{
    public int ModelId { get; set; }
    public int BrandId { get; set; }
    public int PlateColorId { get; set; }
    public int LicenceTypeId { get; set; }
    public string ModelYear { get; set; }
    public Guid CustomerId { get; set; }
}
