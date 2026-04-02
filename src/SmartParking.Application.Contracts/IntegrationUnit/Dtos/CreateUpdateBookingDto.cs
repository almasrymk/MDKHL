using SmartParking.Cutomers;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;

namespace SmartParking.IntegrationUnit.Dtos;

public class CreateUpdateBookingDto
{
    public string UnitCode { get; set; }
    public Guid? CustomerId { get; set; }
    public CreateUpdateCustomerIntegrationUnitDto Customer { get; set; }
    public int? VehicleId { get; set; }
    public CreateUpdateVehicleIntegrationUnitDto Cars { get; set; }
    public CreateUpdateBookingIntegreationUnitDto Booking { get; set; }
    public CreateUpdatePriceIntegreationUnitDto Price { get; set; }

}
