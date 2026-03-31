using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.IntegrationUnit.Dtos;

public class CreateUpdateBookingIntegrationDetailsDto
{
    public string UnitCode { get; set; }
    public DateTime ReservationDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
}
