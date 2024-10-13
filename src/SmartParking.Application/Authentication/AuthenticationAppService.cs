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

    public AuthenticationAppService(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        IOpenIddictTokenManager tokenManager,
        IHttpContextAccessor httpContext,
        IRepository<IdentityUser, Guid> userRepository,
        IIdentityRoleRepository roleRepository,
        IConfiguration config
        )
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _tokenManager = tokenManager;
        _httpContext = httpContext;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _config = config;
    }
    /// <inheritdoc/>
    public async Task<forgetpasswordDto> ForgetPasswordAsync(string phone)
    {
        try
        {
            var user = await CheckPhoneISFound(phone);
            return new forgetpasswordDto
            {
                Success = true,
                UserId = user.Id.ToString(),
            };
        }
        catch (Exception ex)
        {
            return new forgetpasswordDto
            {
                Success = false,
                ErrorMessage = ex.Message
            };
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
    public async Task<UserDataDto?> GetUserDataAsync()
    {

        var Id = CurrentUser.FindClaim(OpenIddictConstants.Claims.Subject)?.Value;
        var user = await _userManager.FindByIdAsync(Id);

        return new UserDataDto
        {
            Success = true,
            FullName = user.UserName,
            Image = $"{_httpContext.HttpContext.Request.Scheme}://{_httpContext.HttpContext.Request.Host}{_httpContext.HttpContext.Request.PathBase}/images/users/149071.png"
        };
    }
    /// <inheritdoc/>
    [RemoteService(IsEnabled = false)]
    public async Task<LoginResultDto> LoginAsync(LoginInputDto input)
    {


        // Step 1: Validate the user's credentials
        var user = await _userManager.FindByNameAsync(input.Username);
        if (user == null || !await _userManager.CheckPasswordAsync(user, input.Password))
        {
            return new LoginResultDto
            {
                Success = false,
                ErrorMessage = "Invalid credentials."
            };
        }
        await _signInManager.SignInAsync(user, isPersistent: false);

        var claims = new List<Claim>
        {
            new Claim(OpenIddictConstants.Claims.Subject, user.Id.ToString()),
            new Claim(OpenIddictConstants.Claims.Username, user.UserName),
            // Add additional claims as needed
        };
        var identity = new ClaimsIdentity(claims, TokenValidationParameters.DefaultAuthenticationType);
        var principal = new ClaimsPrincipal(identity);
        var roles = await _userManager.GetRolesAsync(user);
        return new LoginResultDto
        {
            Success = true,
            AccessToken = await GenerateTokenAsync(user) ?? string.Empty,
            Role = roles.FirstOrDefault(),
            Baseurl = $"{_httpContext.HttpContext.Request.Scheme}://{_httpContext.HttpContext.Request.Host}{_httpContext.HttpContext.Request.PathBase}",
            TenantId = CurrentTenant.Id.ToString()
        };
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
    public async Task<RegisterResultDto> RegisterAsync(RegisterInputDto input)
    {

        try
        {
            await CheckPhoneIsUnique(input.Phone);
            var user = new IdentityUser(GuidGenerator.Create(), input.UserName, input.Email, CurrentTenant.Id);
            user.SetProperty("UserTypeId", 3);
            var createdUser = await _userManager.CreateAsync(user, input.Password);
            await _userManager.SetPhoneNumberAsync(user, input.Phone);
            if (!createdUser.Succeeded)
            {
                throw new Exception(message: createdUser.Errors.FirstOrDefault()?.Description);
            }

            await _userManager.AddToRoleAsync(user, RolesType.Customer.ToString());

            var claims = new List<Claim>
        {
            new Claim(OpenIddictConstants.Claims.Subject, user.Id.ToString()),
            new Claim(OpenIddictConstants.Claims.Username, user.UserName),
            // Add additional claims as needed
        };
            var identity = new ClaimsIdentity(claims, TokenValidationParameters.DefaultAuthenticationType);
            var principal = new ClaimsPrincipal(identity);
            return new RegisterResultDto
            {
                Success = true,
                AccessToken = await GenerateTokenAsync(user) ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            return new RegisterResultDto
            {
                Success = false,
                ErrorMessage = ex.Message,
            };
        }



    }
    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user for whom the token will be generated.</param>
    /// <returns>A string representing the generated JWT token.</returns>
    private async Task<string> GenerateTokenAsync(IdentityUser user)
    {
        var principal = await _signInManager.CreateUserPrincipalAsync(user);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("aP@$$w0rD1234SmartParking5678MDKHL"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(principal.Claims),
            Expires = DateTime.Now.AddHours(1),
            SigningCredentials = creds
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
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

    public async Task<BaseResultDto> ChangePassword(ChangePasswordInputDto input)
    {

        var Id = CurrentUser.FindClaim(OpenIddictConstants.Claims.Subject)?.Value;
        var user = await _userManager.FindByIdAsync(Id);
        if (await _userManager.CheckPasswordAsync(user, input.CurrentPassword))
        {
            await _userManager.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword);
        }
        else
        {
            return new BaseResultDto
            {
                Success = false,
                ErrorMessage = "Incorrect password."
            };

        }

        return new BaseResultDto
        {
            Success = true
        };
    }
    /// <inheritdoc/>
    public async Task LogOutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}
