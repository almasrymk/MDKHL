using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Permissions.SubsctriptionPermissions;

public class SubscriptionPermissions
{

    public const string PropertyName = ".Subscription";
    public const string GroupName = SmartParkingPermissions.GroupName;
    public const string Default = GroupName + PropertyName;
    public const string Create = Default + SmartParkingPermissions.Create;
    public const string Edit = Default + SmartParkingPermissions.Edit;
    public const string Delete = Default + SmartParkingPermissions.Delete;
}
