using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using IdentityUser = Volo.Abp.Identity.IdentityUser;
using Volo.Abp.Application.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.IO;
using SmartParking.Entities.Organizations;
using Volo.Abp.Domain.Entities;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using Polly;
using Microsoft.AspNetCore.Http;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Data;
using SmartParking.Enums;
using Microsoft.Extensions.Configuration;
using Volo.Abp;
using Volo.Abp.TenantManagement;
using static Volo.Abp.TenantManagement.TenantManagementPermissions;
using SmartParking.Results;
using Microsoft.Extensions.Options;
using OpenIddict.Server;

namespace SmartParking.Authentication;
// Service Implementation

/// <inheritdoc/>
public class AuthenticationAppService : ApplicationService, IAuthenticationAppService
{
    
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IOpenIddictTokenManager _tokenManager;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    IIdentityRoleRepository _roleRepository;
    private readonly IConfiguration _config;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly IOptionsMonitor<OpenIddictServerOptions> _oidcOptions;

    public AuthenticationAppService(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        IOpenIddictTokenManager tokenManager,
        IHttpContextAccessor httpContext,
        IRepository<IdentityUser, Guid> userRepository,
        IIdentityRoleRepository roleRepository,
        IConfiguration config,
        IRepository<Tenant,Guid> tenantRepository,
        IOptionsMonitor<OpenIddictServerOptions> oidcOptions
        )
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _tokenManager = tokenManager;
        _httpContext = httpContext;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _config = config;
        _tenantRepository = tenantRepository;
        _oidcOptions = oidcOptions;    }
    /// <inheritdoc/>
    public async Task<Tenant> GetTenantByNameDtoAsync(string name)
    {
        var query = await _tenantRepository.GetQueryableAsync();
       query=query.Where(t => t.Name == name);
        var tenant = await AsyncExecuter.FirstOrDefaultAsync(query);
        return tenant;

    }
    /// <inheritdoc/>
    public async Task<Result<forgetpasswordDto>> ForgetPasswordAsync(string phone)
    {
        try
        {
            var user = await CheckPhoneISFound(phone);
            return Result<forgetpasswordDto>.Success(new forgetpasswordDto
            {
                UserId = user.Id.ToString(),
            });
        }
        catch (Exception ex)
        {
            return  Result<forgetpasswordDto>.Error(errorMessages : ex.Message);
        }

    }

    private async Task<IdentityUser> CheckPhoneISFound(string phone)
    {
        var user = await _userRepository.FirstOrDefaultAsync(u => u.PhoneNumber == phone);

        if (user == null)
        {
            throw new ValidationException("User with this phone number does Not exist.");
        }
        return user;
    }

    /// <inheritdoc/>
    public async Task<Result<UserDataDto>> GetUserDataAsync()
    {
        

        var userName = CurrentUser.UserName;
        if(userName.IsNullOrEmpty())
        {
            return Result<UserDataDto>.NotFound();
        }
        var user = await _userManager.FindByNameAsync(userName);
        if(user is not null)
        {
            return Result<UserDataDto>.Success(new UserDataDto
            {
                FullName = user.UserName,
                Image = $"{_httpContext.HttpContext.Request.Scheme}://{_httpContext.HttpContext.Request.Host}{_httpContext.HttpContext.Request.PathBase}/images/users/149071.png"
            });
        }
        return Result<UserDataDto>.NotFound();

    }
    /// <inheritdoc/>
    public async Task<Result<LoginResultDto>> LoginAsync(LoginInputDto input)
    {


        // Step 1: Validate the user's credentials
        var user = await _userManager.FindByNameAsync(input.Username);
        if (user == null || !await _userManager.CheckPasswordAsync(user, input.Password))
        {
            return Result<LoginResultDto>.Invalid(new Results.ValidationError
            {
                ErrorCode = "403",
                ErrorMessage = "Invalid credentials."
            });
            
        }
        await _signInManager.SignInAsync(user, isPersistent: false);


        var roles = await _userManager.GetRolesAsync(user);
        return Result<LoginResultDto>.Success(new LoginResultDto
        {
            AccessToken = await GenerateTokenAsync(user) ?? string.Empty,
            Role = roles.FirstOrDefault(),
            Baseurl = $"{_httpContext.HttpContext.Request.Scheme}://{_httpContext.HttpContext.Request.Host}{_httpContext.HttpContext.Request.PathBase}",
            TenantId = CurrentTenant.Id.ToString()
        });
       
    } 
    /// <inheritdoc/>
    [RemoteService(IsEnabled = false)]
    public async Task<IdentityUser> LoginForWebAsync(LoginInputDto input)
    {


        // Step 1: Validate the user's credentials
        var user = await _userManager.FindByNameAsync(input.Username);
        
        if (user == null || !await _userManager.CheckPasswordAsync(user, input.Password))
        {
            return user;
        }
        await _signInManager.SignInAsync(user, isPersistent: false);

       
        return user;
    }
    /// <inheritdoc/>
    public async Task<Result<RegisterResultDto>> RegisterAsync(RegisterInputDto input)
    {

        try
        {
            await CheckPhoneIsUnique(input.Phone);
            var user = new IdentityUser(GuidGenerator.Create(), input.UserName, input.Email, CurrentTenant.Id);
            user.SetProperty("OtpCode", null);
            user.SetProperty("OtpExpirationDate", DateTime.Now);
            user.SetProperty("UserTypeId", 3);
            var createdUser = await _userManager.CreateAsync(user, input.Password);
            await _userManager.SetPhoneNumberAsync(user, input.Phone);
            if (!createdUser.Succeeded)
            {
                throw new Exception(message: createdUser.Errors.FirstOrDefault()?.Description);
            }

            await _userManager.AddToRoleAsync(user, RolesType.Customer.ToString());

        //    var claims = new List<Claim>
        //{
        //    new Claim(OpenIddictConstants.Claims.Subject, user.Id.ToString()),
        //    new Claim(OpenIddictConstants.Claims.Username, user.UserName),
        //    // Add additional claims as needed
        //};
        //    var identity = new ClaimsIdentity(claims, TokenValidationParameters.DefaultAuthenticationType);
        //    var principal = new ClaimsPrincipal(identity);
            var AccessToken = await GenerateTokenAsync(user) ?? string.Empty;
            return Result<RegisterResultDto>.Success(new RegisterResultDto
            {
                AccessToken = AccessToken,
            });
        }
        catch (Exception ex)
        {
            return Result<RegisterResultDto>.Error(ex.Message);
            
        }



    }
    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user for whom the token will be generated.</param>
    /// <returns>A string representing the generated JWT token.</returns>
    private async Task<string> GenerateTokenAsync(IdentityUser user)
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
    /// <summary>
    /// Checks if the provided phone number is unique and associated with a user.
    /// </summary>
    /// <param name="phone">The phone number to check for uniqueness.</param>
    /// <returns>The <see cref="IdentityUser"/> associated with the phone number if it exists.</returns>
    /// <exception cref="ValidationException">Thrown when the phone number is not associated with any user.</exception>
    private async Task<IdentityUser> CheckPhoneIsUnique(string phone)
    {
        var user = await _userRepository.FirstOrDefaultAsync(u => u.PhoneNumber == phone);

        if (user != null)
        {
            throw new ValidationException("User with this phone number does  exist.");
        }
        return user;

    }
    /// <inheritdoc/>

    public async Task<Result> ChangePassword(ChangePasswordInputDto input)
    {

        var Id = CurrentUser.FindClaim(OpenIddictConstants.Claims.Subject)?.Value;
        var user = await _userManager.FindByIdAsync(Id);
        if (await _userManager.CheckPasswordAsync(user, input.CurrentPassword))
        {
            await _userManager.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword);
        }
        else
        {
            return Result.Error("Incorrect password.");

        }

        return Result.Success();
    }
    /// <inheritdoc/>
    public async Task LogOutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}
