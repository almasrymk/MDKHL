using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;

namespace SmartParking.Utilities;

public interface IAuthenticationUtility : ITransientDependency
{
    Task<IdentityRole?> GetRoleByNameAndTenant(string RoleName, Guid? TenantId);
    Task CheckNationalId(IdentificationTypes identification,string NationalId);
    Task CheckIfNationalIDIsUnique( string NationalId);
    Task CheckPhoneNumberIsValid(string phoneNumber);
    Task<IdentityUser> CheckPhoneISFound(string phone);
    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user for whom the token will be generated.</param>
    /// <returns>A string representing the generated JWT token.</returns>
    Task<string> GenerateTokenAsync(IdentityUser user);
}
