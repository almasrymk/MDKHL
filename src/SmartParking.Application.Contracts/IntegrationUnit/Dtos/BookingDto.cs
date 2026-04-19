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

}
