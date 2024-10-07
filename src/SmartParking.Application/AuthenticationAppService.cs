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
using System.Linq;
using System.IdentityModel.Tokens.Jwt;

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
        return new LoginResult
        {
            Success = true,
            AccessToken = await GenerateTokenAsync(user) ?? string.Empty 
        };
    }

    public async Task<string> GenerateTokenAsync(IdentityUser user)
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

}
