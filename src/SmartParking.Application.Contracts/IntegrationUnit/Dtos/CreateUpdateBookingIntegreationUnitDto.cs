using System;

namespace SmartParking.IntegrationUnit.Dtos;

public class CreateUpdateBookingIntegreationUnitDto
{
    public string BookingId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int NoOfDays { get; set; }
}
