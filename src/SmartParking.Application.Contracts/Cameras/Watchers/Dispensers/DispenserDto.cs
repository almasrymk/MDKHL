using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Cameras.Watchers.Dispensers;

public class DispenserDto : WatcherDto
{
    public int TotalRemainNumber { get; set; }
    public string GarageNameAr { get; set; }
    public string GarageNameEn { get; set; }
    public string OrganizationNameAr { get; set; }
    public string OrganizationNameEn { get; set; }
}
