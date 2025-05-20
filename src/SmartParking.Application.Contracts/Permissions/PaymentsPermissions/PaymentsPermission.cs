using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Permissions.PaymentsPermissions;

public class PaymentsPermission
{
    public const string PropertyName = ".Payment";
    public const string GroupName = SmartParkingPermissions.GroupName;
    public const string Default = GroupName + PropertyName;
    public const string View = Default + SmartParkingPermissions.View;
}
