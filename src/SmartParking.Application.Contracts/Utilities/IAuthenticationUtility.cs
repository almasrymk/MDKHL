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
}
