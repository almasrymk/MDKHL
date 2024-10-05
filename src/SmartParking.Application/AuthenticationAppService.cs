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
using SmartParking.Authentication;
using Volo.Abp.Identity;
using System.Linq;

namespace SmartParking;
// Service Implementation

public class AuthenticationAppService : ApplicationService, IAuthenticationAppService
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IOpenIddictTokenManager _tokenManager;
    private readonly IIdentityRoleAppService _roleAppService;

    public AuthenticationAppService(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        IOpenIddictTokenManager tokenManager,
        IIdentityRoleAppService roleAppService
        )
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _tokenManager = tokenManager;
        _roleAppService = roleAppService;
    }
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
            AccessToken = await GenerateTokenAsync(user) ?? string.Empty ,
            Role = roles.FirstOrDefault(),
        };
    }

    public async Task<RegisterResultDto> RegisterAsync(RegisterInputDto input)
    {
        var user = new IdentityUser(GuidGenerator.Create(), input.UserName, input.Email, CurrentTenant.Id);
        var roles = await _roleAppService.GetAllListAsync();
        ;
        if (roles.Items.FirstOrDefault(x => x.Name == "user") is null)
        {
            var role = new IdentityRoleCreateDto { Name = "user",IsDefault=true,IsPublic=true};

           await _roleAppService.CreateAsync(role);
        }
         

        try
        {
           var createdUser= await _userManager.CreateAsync(user, input.Password);

            if (!createdUser.Succeeded)
            {
                throw new Exception(message:createdUser.Errors.FirstOrDefault()?.Description);
            }
            
            await _userManager.AddToRoleAsync(user, "user");

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

}
