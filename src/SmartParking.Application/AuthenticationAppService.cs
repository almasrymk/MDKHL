using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Volo.Abp.Identity;
using IdentityUser = Volo.Abp.Identity.IdentityUser;
using Volo.Abp.Application.Services;
using Volo.Abp.Users;
using Abp.Json;

namespace SmartParking;
// Service Implementation

public class AuthenticationAppService : ApplicationService, IAuthenticationAppService
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IOpenIddictTokenManager _tokenManager;

    public AuthenticationAppService(
        SignInManager<Volo.Abp.Identity.IdentityUser> signInManager,
        UserManager<Volo.Abp.Identity.IdentityUser> userManager,
        IOpenIddictTokenManager tokenManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _tokenManager = tokenManager;
    }

    public async Task<LoginResult> LoginAsync(LoginInput input)
    {
        // Step 1: Validate the user's credentials
        var user = await _userManager.FindByEmailAsync(input.Username);

        if (user == null || !await _userManager.CheckPasswordAsync(user, input.Password))
        {
            return new LoginResult
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

   
        var descriptor = new OpenIddictTokenDescriptor
        {
            Subject = user.Id.ToString(),
            Principal = principal,
            CreationDate = DateTimeOffset.UtcNow,
            ExpirationDate = DateTimeOffset.UtcNow.AddHours(1),
            Payload = user.ToJsonString(),
            Type = OpenIddictConstants.TokenTypes.Bearer.ToString(),
        };

     
        var token = await _tokenManager.CreateAsync(descriptor);

        return new LoginResult
        {
            Success = true,
            AccessToken = token?.ToString() ?? string.Empty  //not work true but login have a bug
        };
    }
}
