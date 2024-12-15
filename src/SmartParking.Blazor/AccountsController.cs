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

namespace SmartParking.Blazor;
[Route("api/[controller]")]
[ApiController]
public class AccountsController : ControllerBase
{
    private readonly IAuthenticationAppService _authenticationApp;
    private readonly ITenantAppService _tenantAppService;
    private readonly ICurrentTenant currentTenant;
    private readonly ICurrentUser _currentuser;

    public AccountsController(
        IAuthenticationAppService authenticationApp,
        ITenantAppService tenantAppService,
        ICurrentTenant currentTenant,
        ICurrentUser currentuser
        )
    {
        _authenticationApp = authenticationApp;
        _tenantAppService = tenantAppService;
        this.currentTenant = currentTenant;
        _currentuser = currentuser;
    }

    [HttpGet]
    public async Task<IActionResult> Login(string username , string password)
    {
        
        var tenant =await _authenticationApp.GetTenantByNameDtoAsync(username);
        currentTenant.Change(tenant?.Id??null);
        LoginInputDto loginInputDto = new LoginInputDto()
        {
            Username = username,
            Password = password
        };
       var userLogin= await _authenticationApp.LoginForWebAsync(loginInputDto);

        if (_currentuser.IsInRole(RolesType.Customer.ToString())) {

           return await Logout();
        
        }
        if (userLogin is not null)
        {
            return LocalRedirect("/");
            
        }
        
        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return LocalRedirect("/Account/Login");
        
    }

    [HttpGet("/logOut")]
    public async Task<ActionResult> Logout()
    {
        await _authenticationApp.LogOutAsync();
        return LocalRedirect("/Account/Login");
    }


}


