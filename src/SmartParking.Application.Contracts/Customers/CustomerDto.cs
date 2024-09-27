using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Cutomers;

public class CustomerDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string MobileNumber { get; set; }
    public string NationalId { get; set; }
    public IdentificationTypes identificationTypes { get; set; }
    public List<VehicleDto> Vehicles { get; set; }
}

public class VehicleDto
{
    public string LicensePlate { get; set; }
    public string Model { get; set; }
    public string Make { get; set; }
    public int Year { get; set; }
}
