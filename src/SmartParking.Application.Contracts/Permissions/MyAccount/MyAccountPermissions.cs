using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Permissions.MyAccount;

public class MyAccountPermissions
{
    public const string PropertyName = ".MyAccount";
    public const string GroupName = SmartParkingPermissions.GroupName;
    public const string Default = GroupName + PropertyName;
    public const string Edit = Default + SmartParkingPermissions.Edit;
}
