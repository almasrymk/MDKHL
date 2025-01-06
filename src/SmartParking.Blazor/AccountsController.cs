using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Authentication;
using Microsoft.AspNetCore.Authentication;
using Abp.Domain.Repositories;
using Volo.Abp.TenantManagement;
using Volo.Abp.MultiTenancy;
using System;
using Volo.Abp.Users;
using SmartParking.Enums;
using MudBlazor;
using Microsoft.AspNetCore.Identity;
using IdentityUser =Volo.Abp.Identity.IdentityUser;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
namespace SmartParking.Blazor;
[Route("api/[controller]")]
[ApiController]
public class AccountsController : ControllerBase
{
    private readonly IAuthenticationAppService _authenticationApp;
    private readonly ICurrentTenant currentTenant;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountsController(
        IAuthenticationAppService authenticationApp,
        ICurrentTenant currentTenant,
        SignInManager<IdentityUser> signInManager
        )
    {
        _authenticationApp = authenticationApp;
        this.currentTenant = currentTenant;
        _signInManager = signInManager;
    }

    [HttpGet("/logIn/{token}")]
    public async Task<IActionResult> Login(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var claimsIdentity = new ClaimsIdentity(jwtToken.Claims, "Bearer");
            var userId = claimsIdentity.FindFirst("userId")?.Value;
            var user = await _authenticationApp.IsUserFoundAsync(userId);
            currentTenant.Change(user.TenantId);
            await _signInManager.SignInAsync(user, false);

            return LocalRedirect("/");
        }
        catch
        {
           return LocalRedirect($"/Account/Login");
        }
    }

    [HttpGet("/logOut")]
    public async Task<ActionResult> Logout()
    {
        await _authenticationApp.LogOutAsync();
        return LocalRedirect($"/Account/Login");
    }


}


