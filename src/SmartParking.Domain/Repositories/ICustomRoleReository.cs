
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace SmartParking.Repositories;

public interface ICustomRoleReository :IRepository<IdentityRole, Guid>
{
    Task<IdentityRole?> FindRoleByTenantAsync(string role, Guid? tenantId);
}
 