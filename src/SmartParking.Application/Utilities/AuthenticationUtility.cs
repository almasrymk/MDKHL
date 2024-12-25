using Microsoft.EntityFrameworkCore;
using SmartParking.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;

namespace SmartParking.Utilities;

public class AuthenticationUtility : IAuthenticationUtility , ITransientDependency
{
    private readonly ICustomRoleReository _roleReository;

    public AuthenticationUtility(ICustomRoleReository roleReository)
    {
        _roleReository = roleReository;
    }

    public async Task<IdentityRole?> GetRoleByNameAndTenant(string RoleName, Guid? TenantId)
    {
        var query = await _roleReository.GetQueryableAsync();
        query = query.Where(r => r.Name == RoleName && r.TenantId == TenantId);
        var role = await query.FirstOrDefaultAsync();
        return role;
    }
}
