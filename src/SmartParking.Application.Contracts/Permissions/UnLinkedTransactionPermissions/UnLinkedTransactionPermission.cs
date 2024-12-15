using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Permissions.UnLinkedTransactionPermissions;

public class UnLinkedTransactionPermission
{
    public const string PropertyName = ".UnLinkedTransaction";
    public const string GroupName = SmartParkingPermissions.GroupName;
    public const string Default = GroupName + PropertyName;
}