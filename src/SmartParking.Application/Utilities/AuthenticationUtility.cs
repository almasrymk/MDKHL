using Microsoft.EntityFrameworkCore;
using SmartParking.Enums;
using SmartParking.Localization;
using SmartParking.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.Linq;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;

namespace SmartParking.Utilities;

public class AuthenticationUtility : IAuthenticationUtility , ITransientDependency
{
    private readonly ICustomRoleReository _roleReository;
    private readonly IMyUserRepository _userRepository;

    public AuthenticationUtility(ICustomRoleReository roleReository, IMyUserRepository userRepository)
    {
        _roleReository = roleReository;
        _userRepository = userRepository;
    }

    public async Task CheckNationalId(IdentificationTypes identification, string NationalId)
    {
        if (identification == IdentificationTypes.NationalID)
        {
            var nationalIdPattern = new Regex(@"^(1|2)\d{9}$");
            if (!nationalIdPattern.IsMatch(NationalId))
            {
                throw new UserFriendlyException(LocalizationHelper.GetLocalizedString("NationIdValidation"));
            }
        }
        else if (identification == IdentificationTypes.Passport)
        {
            var passportPattern = new Regex(@"^(?=.*[a-zA-Z]{0,1})[a-zA-Z0-9]{6,9}$");
            if (!passportPattern.IsMatch(NationalId))
            {
                throw new UserFriendlyException(LocalizationHelper.GetLocalizedString("PassportValidation"));
            }
        }
    }

    public async Task CheckIfNationalIDIsUnique(string NationalId)
    {
        var query = await _userRepository.GetQueryableAsync();
        query = query.Where(x => EF.Property<string>(x, "NationalId") == NationalId);
        var result = await query.FirstOrDefaultAsync();
        if (result is not null)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("NationalId") + " " + LocalizationHelper.GetLocalizedString("DuplicateValidation"));

        }

    }

    public async Task<IdentityRole?> GetRoleByNameAndTenant(string RoleName, Guid? TenantId)
    {
        var query = await _roleReository.GetQueryableAsync();
        query = query.Where(r => r.Name == RoleName && r.TenantId == TenantId);
        var role = await query.FirstOrDefaultAsync();
        return role;
    }
}
