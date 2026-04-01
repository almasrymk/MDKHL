using SmartParking.Cutomers;
using SmartParking.Enums;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;

namespace SmartParking.IntegrationUnit.Dtos;

public class BookingDto
{
    public Status Status { get; set; } = Status.Active;
    public Guid CustomerId { get; set; }
    public string PackageId { get; set; }
    public string PackageNumber { get; set; }
    public ValidityDto Validity { get; set; }
    public QrDto Qr { get; set; }
    public LinksDto Links { get; set; }
    //public CustomerDto CustomerDto { get; set; }
    //public VehicleDto VehicleDto { get; set; }
    //public string UnitCode { get; set; }
    //public DateTime ActivationDate { get; set; }
    //public DateTime DeactivationDate { get; set; }
    //public decimal Price { get; set; }
    //public decimal Discount { get; set; }
}
