using SmartParking.Cutomers;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;

namespace SmartParking.IntegrationUnit.Dtos;

public class CreateUpdateBookingDto
{
    public Guid? CustomerId { get; set; }
    public CreateUpdateCustomerIntegrationUnitDto CustomerDto { get; set; }
    public int? VehicleId { get; set; }
    public CreateUpdateVehicleIntegrationUnitDto VehicleDto { get; set; }
    public CreateUpdateBookingIntegrationDetailsDto IntegrationDetailsDto { get; set; }

}
