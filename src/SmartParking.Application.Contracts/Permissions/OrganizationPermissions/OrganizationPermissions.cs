using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Permissions.OrganizationPermissions;

public class OrganizationPermissions
{
    public const string PropertyName = ".Organizations";
    public const string GroupName = SmartParkingPermissions.GroupName;
    public const string Default = GroupName + PropertyName;
    public const string Create = Default + SmartParkingPermissions.Create;
    public const string Edit = Default + SmartParkingPermissions.Edit;
    public const string Delete = Default + SmartParkingPermissions.Delete;
    public const string ChanageStatus = Default + SmartParkingPermissions.ChanageStatus;
}
