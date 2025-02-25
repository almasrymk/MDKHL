using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Permissions.UserShiftsPermissions;

public class UserShiftPermission
{
    public const string PropertyName = ".UserShifts";
    public const string GroupName = SmartParkingPermissions.GroupName;
    public const string Default = GroupName + PropertyName;
    public const string Assign = Default + ".Assign";
    public const string ViewUserActive = Default + ".ViewUserActive";
    public const string ViewUserShift = Default + ".ViewUserShift";
}
