using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Authentication;
using SmartParking.Results;
using System.Threading.Tasks;
using Volo.Abp.Users;

namespace SmartParking.Controllers;

[Route("api/mobile/[controller]")]
[ApiController]
public class AuthenticationsController : ControllerBase
{
    private readonly IAuthenticationAppService _authenticationApp;
    private readonly ICurrentUser _user;

    public AuthenticationsController(IAuthenticationAppService authentication,ICurrentUser user)
    {
        _authenticationApp = authentication;
        _user = user;
    }
    [HttpPost("login")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<Result<LoginResultDto>> LoginAsync(LoginInputDto input)
    {
        if (_user.IsAuthenticated)
        {
            return Result<LoginResultDto>.Invalid(new ValidationError
            {
                ErrorCode = "400",
                ErrorMessage = "You are already logged in."
            });
        }
        return await _authenticationApp.LoginAsync(input);
    }
    [HttpPost("register")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<Result<RegisterResultDto>> RegisterAsync(RegisterInputDto input)
    {
        if (_user.IsAuthenticated)
        {
            return Result<RegisterResultDto>.Invalid(new ValidationError
            {
                ErrorCode = "400",
                ErrorMessage = "You are already logged in."
            });
        }
        return await _authenticationApp.RegisterAsync(input);
    }
    [HttpPost("forgetPassword")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<Result<forgetpasswordDto>> ForgetPasswordAsync(string phone)
    {
        if (_user.IsAuthenticated)
        {
            return Result<forgetpasswordDto>.Invalid(new ValidationError
            {
                ErrorCode = "400",
                ErrorMessage = "You are already logged in."
            });
        }
        return await _authenticationApp.ForgetPasswordAsync(phone);
    }
    [HttpGet("userData")]
    [Authorize]
    public async Task<Result<UserDataDto>> GetUserDataAsync()
    {
        return await _authenticationApp.GetUserDataAsync();
    }
    [HttpPost("changePassword")]
    [IgnoreAntiforgeryToken]
    public async Task<Result> ChangePassword(ChangePasswordInputDto input)
    {
        return await _authenticationApp.ChangePassword(input);
    }
}
