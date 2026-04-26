using SmartParking.Cutomers;
using SmartParking.Enums;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;

namespace SmartParking.IntegrationUnit.Dtos;

public class BookingDto
{
    public Status Status { get; set; } = Status.Active;

    public string PhoneNumber { get; set; }
    public List<PronightPackageResponseDto> Package { get; set; }
    public ValidityDto Validity { get; set; }
    public List<QrDto> Qr { get; set; }
    public LinksDto Links { get; set; }

}
