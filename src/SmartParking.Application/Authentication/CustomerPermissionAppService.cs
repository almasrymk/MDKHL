using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SimpleStateChecking;

namespace SmartParking.Authentication;

public class CustomerPermissionAppService : PermissionAppService, ITransientDependency, ICustomerPermissionAppService
{
    public CustomerPermissionAppService(IPermissionManager permissionManager, IPermissionDefinitionManager permissionDefinitionManager, IOptions<PermissionManagementOptions> options, ISimpleStateCheckerManager<PermissionDefinition> simpleStateCheckerManager) : base(permissionManager, permissionDefinitionManager, options, simpleStateCheckerManager)
    {
    }
    public override async Task UpdateAsync(string providerName, string providerKey, UpdatePermissionsDto input)
    {
        

        foreach (var permissionDto in input.Permissions)
        {
            if(permissionDto.Name.Contains("SmartParking"))
            await PermissionManager.SetAsync(permissionDto.Name, providerName, providerKey, permissionDto.IsGranted);
        }
    }
}
