using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Identity;

namespace SmartParking.Lookups.User
{
    public class IdentityUserRoleDto : IdentityUserRole
    {
        public IdentityUserRoleDto(Guid userId, Guid roleId, Guid? tenantId)
        {
            this.UserId = userId;
            this.RoleId = roleId;
            this.TenantId = tenantId;
        }

        public IdentityUserRole GetModel()
        {
            return (IdentityUserRole)this;
        }
    }
}