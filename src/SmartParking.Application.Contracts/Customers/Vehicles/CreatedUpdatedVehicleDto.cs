using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Customers.Vehicles;

public class CreatedUpdatedVehicleDto
{
    public string LicensePlate { get; set; }
    public string Model { get; set; }
    public string Make { get; set; }
    public int Year { get; set; }
}
