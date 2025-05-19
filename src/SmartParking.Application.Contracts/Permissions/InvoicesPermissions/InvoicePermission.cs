using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Permissions.InvoicesPermissions;

public class InvoicePermission
{
    public const string PropertyName = ".Invoice";
    public const string GroupName = SmartParkingPermissions.GroupName;
    public const string Default = GroupName + PropertyName;
    public const string Confirm = Default + ".Confirm";
    public const string View = Default + SmartParkingPermissions.View;
}
