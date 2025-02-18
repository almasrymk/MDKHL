using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Permissions.IdentityPermissions;

public static class CustomIdentityPermissions
{
    public const string GroupName = "AbpIdentity";

    public static class Roles
    {
        public const string Default = GroupName + ".Roles";
        
        public const string ViewPermissions = Default + ".ViewPermissions";
    }
}
