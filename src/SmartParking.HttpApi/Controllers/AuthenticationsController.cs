using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Authentication;
using SmartParking.Results;
using System;
using System.Threading.Tasks;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;

namespace SmartParking.Controllers;

[Route("api/mobile/[controller]")]
[ApiController]
public class AuthenticationsController : ControllerBase
{
    private readonly IAuthenticationAppService _authenticationApp;
    private readonly IUsersAppService _usersAppService;
    private readonly ICurrentUser _user;
    private readonly ICurrentTenant _currentTenant;


    public AuthenticationsController(IAuthenticationAppService authentication, ICurrentUser user, ICurrentTenant currentTenant, IUsersAppService usersAppService)
    {
        _authenticationApp = authentication;
        _user = user;
        _currentTenant = currentTenant;
        _usersAppService = usersAppService;
    }
    [HttpPost("login")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<Result<LoginResultDto>> LoginAsync(LoginMobileInputDto input)
    {
        try
        {
            var tenant = await _authenticationApp.GetTenantByPhoneNumberDtoAsync(input.PhoneNumber);
            _currentTenant.Change(tenant?.Id ?? null);
            if (_user.IsAuthenticated)
            {
                return Result<LoginResultDto>.Invalid(new ValidationError
                {
                    ErrorCode = "400",
                    ErrorMessage = "You are already logged in."
                });
            }
            return await _authenticationApp.LoginAsync(input);
        }catch(Exception ex)
        {
            return Result<LoginResultDto>.Invalid(new Results.ValidationError
            {
                ErrorCode = "403",
                ErrorMessage = "This user not found",
            });
        }
        
    }    
    [HttpPost("userdata")]
    [Authorize]
    public async Task<Result<UserDataDto>> UpdateUser(string phone,string password)
    {
       
        return await _authenticationApp.UpdateUser(phone,password);
    }
    [HttpPost("addprofileimage")]
    [Authorize]
    public async Task<Result<UserDataDto>> AddProfileImage(string image)
    {
        var currentCustomerId=await _authenticationApp.GetCurrentCustomerId();
        var result = await _usersAppService.UpdateImageAsync(currentCustomerId, image);
        return Result.Success( new UserDataDto
        {
            FullName=result.Name,
            Email=result.Email,
            Phone=result.PhoneNumber,
            Image=result.Image,
        });
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
    [HttpPost("setNewPassword")]
    [IgnoreAntiforgeryToken]
    public async Task<Result> SetNewPassword(ChangePasswordInputDto input)
    {
        return await _authenticationApp.SetNewPassword(input);
    }   
    
    [HttpPost("changeImage")]
    [Authorize]
    public async Task<Result<string>> ChangeImage([FromBody]string image)
    {
        var user =  await _usersAppService.UpdateImageAsync(_user.Id, image);
        return Result.Success(user.Image);
    }    
    
    [HttpGet("GetImage")]
    [Authorize]
    public async Task<Result<string?>> GetImage(string image)
    {
        try
        {
            var user = await _usersAppService.GetImage(_user.Id);
            return Result.Success(user);
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
        
    }
}
