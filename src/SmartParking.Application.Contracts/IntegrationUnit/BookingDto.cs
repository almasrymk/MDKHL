using SmartParking.Cutomers;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;

namespace SmartParking.IntegrationUnit;

public class BookingDto
{
    public CustomerDto CustomerDto { get; set; }
    public VehicleDto VehicleDto { get; set; }
    public string UnitCode { get; set; }
    public DateTime ActivationDate { get; set; }
    public DateTime DeactivationDate { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
}
