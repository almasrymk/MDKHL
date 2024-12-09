using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Permissions.GaragePermissions;

public static class GaragePermissions
{
    public const string PropertyName = ".Garages";
    public const string GroupName = SmartParkingPermissions.GroupName;
    public const string Default = GroupName + PropertyName;
    public const string Create = Default + SmartParkingPermissions.Create;
    public const string Edit = Default + SmartParkingPermissions.Edit;
    public const string Delete = Default + SmartParkingPermissions.Delete;
}
