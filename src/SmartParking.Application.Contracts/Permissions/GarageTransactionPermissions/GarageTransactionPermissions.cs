using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Permissions.GarageTransactionPermissions;

public class GarageTransactionPermissions
{
    public const string PropertyName = ".GarageTransaction";
    public const string GroupName = SmartParkingPermissions.GroupName;
    public const string Default = GroupName + PropertyName;
    public const string View = GroupName + SmartParkingPermissions.View;
}

