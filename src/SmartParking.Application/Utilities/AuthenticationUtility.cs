using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using SmartParking.Authentication;
using SmartParking.Enums;
using SmartParking.Localization;
using SmartParking.Repositories;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using IdentityUser = Volo.Abp.Identity.IdentityUser;
using IdentityRole = Volo.Abp.Identity.IdentityRole;
using Microsoft.Extensions.Options;
using OpenIddict.Server;
using Microsoft.AspNetCore.Http;
namespace SmartParking.Utilities;

public class AuthenticationUtility : IAuthenticationUtility , ITransientDependency
{
    private readonly ICustomRoleReository _roleReository;
    private readonly IMyUserRepository _userRepository;
    private readonly IDataFilter _dataFilter;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IOptionsMonitor<OpenIddictServerOptions> _oidcOptions;
    private readonly IHttpContextAccessor _httpContext;

    public AuthenticationUtility(ICustomRoleReository roleReository, IMyUserRepository userRepository,
        IDataFilter dataFilter,
        UserManager<IdentityUser> userManager,
        IOptionsMonitor<OpenIddictServerOptions> oidcOptions,
        IHttpContextAccessor httpContext
        )
    {
        _roleReository = roleReository;
        _userRepository = userRepository;
        _dataFilter = dataFilter;
        _userManager = userManager;
        _oidcOptions = oidcOptions;
        _httpContext = httpContext;
    }

    public async Task CheckNationalId(IdentificationTypes identification, string NationalId)
    {
        if (identification == IdentificationTypes.NationalID)
        {
            var nationalIdPattern = new Regex(@"^(1|2)\d{9}$");
            if (!nationalIdPattern.IsMatch(NationalId))
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("NationIdValidation"));
            }
        }
        else if (identification == IdentificationTypes.Passport)
        {
            var passportPattern = new Regex(@"^(?=.*[a-zA-Z]{0,1})[a-zA-Z0-9]{6,9}$");
            if (!passportPattern.IsMatch(NationalId))
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("PassportValidation"));
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

    public async Task CheckPhoneNumberIsValid(string phoneNumber)
    {
       await Task.Yield();
        var phoneNumberPattern = new Regex(@"(05\d{8}|\+9665\d{8})$");
        if (!phoneNumberPattern.IsMatch(phoneNumber))
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("PhoneValidation"));
        }
    }    

    public async Task<IdentityUser> CheckPhoneISFound(string phone)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var user = await _userRepository.FirstOrDefaultAsync(u => u.PhoneNumber == phone);

            if (user != null)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("User") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
            }
            return user;
        }

    }

    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user for whom the token will be generated.</param>
    /// <returns>A string representing the generated JWT token.</returns>
    public async Task<string> GenerateTokenAsync(IdentityUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>()
        {
            new Claim("sub", user.Id.ToString()),
            new Claim("given_name", user.UserName),
            new Claim("email", user.Email),
            new Claim("role", string.Join(",", roles)),
            new Claim("tenantid", user.TenantId?.ToString() ?? ""),
            new Claim("scope", "address email phone roles profile offline_access SmartParking") //replace Qa with yours

        };
        var options = _oidcOptions.CurrentValue;
        var descriptor = new SecurityTokenDescriptor
        {
            Audience = "SmartParking", // replace with yours,
            EncryptingCredentials = options.DisableAccessTokenEncryption
                ? null
                : options.EncryptionCredentials.First(),
            Expires = null,
            Subject = new ClaimsIdentity(claims, TokenValidationParameters.DefaultAuthenticationType),
            IssuedAt = DateTime.UtcNow,
            Issuer = $"{_httpContext.HttpContext.Request.Scheme}://{_httpContext.HttpContext.Request.Host}{_httpContext.HttpContext.Request.PathBase}", // replace with yours,
            SigningCredentials = options.SigningCredentials.First(),
            TokenType = OpenIddictConstants.JsonWebTokenTypes.AccessToken,
        };

        var accessToken = options.JsonWebTokenHandler.CreateToken(descriptor);

        return accessToken;
    }
}
