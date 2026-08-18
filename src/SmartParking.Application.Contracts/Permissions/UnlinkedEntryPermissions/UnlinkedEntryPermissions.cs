using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Permissions.UnlinkedEntryPermissions;

public class UnlinkedEntryPermissions
{
    public const string PropertyName = ".UnlinkedEntry";
    public const string GroupName = SmartParkingPermissions.GroupName;
    public const string Default = GroupName + PropertyName;
    public const string View = Default + SmartParkingPermissions.View;
    public const string SendAsEntry = Default + SmartParkingPermissions.SendAsEntry;
}
