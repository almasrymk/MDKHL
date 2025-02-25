using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Permissions.DailyShiftsPermissions;

public class DailyShiftsPermission
{
    public const string PropertyName = ".DailyShifts";
    public const string GroupName = SmartParkingPermissions.GroupName;
    public const string Default = GroupName + PropertyName;
}
