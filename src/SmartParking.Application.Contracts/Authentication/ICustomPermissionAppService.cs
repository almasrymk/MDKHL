using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.PermissionManagement;

namespace SmartParking.Authentication;
/// <summary>
/// Interface for cutom pemission service
/// </summary>
public interface ICustomPermissionAppService : IPermissionAppService, ITransientDependency
{
    /// <summary>
    /// Updates the permissions for a specific provider based on the provided provider name and key.
    /// </summary>
    /// <param name="providerName">The name of the provider whose permissions are being updated.</param>
    /// <param name="providerKey">The key identifying the specific provider.</param>
    /// <param name="input">The data to update the permissions with, encapsulated in an <see cref="UpdatePermissionsDto"/>.</param>
    /// <returns>A task that represents the asynchronous update operation.</returns>
    Task UpdateAsync(string providerName, string providerKey, UpdatePermissionsDto input);
    /// <summary>
    /// Retrieves the permission list for a specific provider based on the provider name and key.
    /// </summary>
    /// <param name="providerName">The name of the provider whose permissions are being retrieved.</param>
    /// <param name="providerKey">The key identifying the specific provider.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the <see cref="GetPermissionListResultDto"/> 
    /// containing the list of permissions for the specified provider.
    /// </returns>
    Task<GetPermissionListResultDto> GetAsync(string providerName, string providerKey);
}
