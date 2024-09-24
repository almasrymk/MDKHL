using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Organizations;

public class OrganizationDto
{
    public string Code { get; set; }
    public string EnglishName { get; set; }
    public string CountryName { get; set; }
    public string StateName { get; set; }
    public string CityName { get; set; }
    public int MaxCapacity { get; set; }
    public int NumOfCameras { get; set; }
    public Status Status { get; set; }


}
