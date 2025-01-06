using SmartParking.Entities.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Analysis;

public class GarageTransactionAnalysis
{
    public string GarageName { get; set; }
    public string UserName { get; set; }
    public string CameraType { get; set; }
    public string PlateNumber { get; set; }
    public DateTime? AccurateTime { get; set; }
    public string OrganizationName { get; set; }
}
