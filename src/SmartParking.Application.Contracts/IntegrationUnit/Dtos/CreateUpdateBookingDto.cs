using SmartParking.Cutomers;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;

namespace SmartParking.IntegrationUnit.Dtos;

public class CreateUpdateBookingDto
{
    public string UnitId { get; set; }
    public string? CustomerId { get; set; }
    public CreateUpdateCustomerIntegrationUnitDto Customer { get; set; }
    public CreateUpdateBookingIntegreationUnitDto Booking { get; set; }
    public CreateUpdatePriceIntegreationUnitDto Price { get; set; }

}
