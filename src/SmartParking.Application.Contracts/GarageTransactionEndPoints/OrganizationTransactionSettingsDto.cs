using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.GarageTransactionEndPoints;

public class OrganizationTransactionSettingsDto
{
    public bool HasPackageOnly { get; set; }
    public bool HasFreeSlots { get; set; }
    public bool HasUserAccess { get; set; }
    public bool HasVehicleAccess { get; set; }
}
