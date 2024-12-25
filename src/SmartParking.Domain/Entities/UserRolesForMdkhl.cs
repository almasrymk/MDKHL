using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities;

public class UserRolesForMdkhl
{
    public string RoleName { get; set; }
    public string OrganizationName { get; set; }
    public Guid? TenantId { get; set; }
    public Guid RoleId { get; set; }
}
