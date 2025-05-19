using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Permissions.UserProfile;

public static class UserProfilePermissions
{
    public const string PropertyName = ".EditProfile";
    public const string GroupName = SmartParkingPermissions.GroupName;
    public const string Default = GroupName + PropertyName;
    public const string Edit = Default + SmartParkingPermissions.Edit;
    public const string ChangePassword = Default + SmartParkingPermissions.ChangePassword;
   // public const string View = Default + SmartParkingPermissions.View;
}
