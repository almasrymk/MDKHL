using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MudBlazor;
using SmartParking.Authentication;
using SmartParking.Organizations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartParking.Blazor.Controller;

[Route("api/mobile/[controller]")]
[ApiController]
public class AuthenticationsController : ControllerBase
{
    private readonly IAuthenticationAppService _authenticationApp;

    public AuthenticationsController(IAuthenticationAppService authentication)
    {
        _authenticationApp = authentication;
    }
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<LoginResultDto> LoginAsync(LoginInputDto input)
    {
        return await _authenticationApp.LoginAsync(input);
    }
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<RegisterResultDto> RegisterAsync(RegisterInputDto input)
    {
        return await _authenticationApp.RegisterAsync(input);
    }
    [HttpPost("forgetPassword")]
    [AllowAnonymous]
    public async Task<forgetpasswordDto> ForgetPasswordAsync(string phone)
    {
        return await _authenticationApp.ForgetPasswordAsync(phone);
    }
    [HttpGet("userData")]
    [Authorize]
    public async Task<UserDataDto> GetUserDataAsync()
    {
        return await _authenticationApp.GetUserDataAsync();
    }
    [HttpPost("changePassword")]
    [Authorize]
    public async Task<BaseResultDto> ChangePassword(ChangePasswordInputDto input)
    {
        return await _authenticationApp.ChangePassword(input);
    }
}
