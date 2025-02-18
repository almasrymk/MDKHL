using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.PermissionManagement;

namespace SmartParking.Authentication;

public interface ICustomPermissionAppService : IPermissionAppService, ITransientDependency
{
    Task UpdateAsync(string providerName, string providerKey, UpdatePermissionsDto input);
    Task<GetPermissionListResultDto> GetAsync(string providerName, string providerKey);
}
