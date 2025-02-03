using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Authentication;
using SmartParking.Mobile.Keepers;
using SmartParking.Results;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Users;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;
using UserDataDto = SmartParking.Authentication.UserDataDto;
namespace SmartParking.Controllers;

[Route("api/Keeper/User")]
[ApiController]
public class UserKeeperController : AbpController
{
    private readonly IUsersAppService _userAppService;
    private readonly IAuthenticationAppService _authenticationAppService;
    private readonly ICurrentUser _user;

    public UserKeeperController(IUsersAppService userAppService
        , IAuthenticationAppService authenticationAppService,
        ICurrentUser user)
    {
        _userAppService = userAppService;
        _authenticationAppService = authenticationAppService;
        _user = user;
    }

    [HttpPost("setNewPassword")]
    [Authorize]
    public async Task<Result> SetNewPassword(ChangePasswordInputDto input)
    {
        return await _authenticationAppService.SetNewPassword(input);
    }

    [HttpPost("profileUser")]
    [Authorize]
    public async Task<Result<UserDataDto>>UpdateUserAsync(UserDataDto input)
    {
        return await _userAppService.UpdateProfileDataMobileAsync(_user.Id, input);
    }

    [HttpGet]
    [Authorize]
    public async Task<Result<UserDataDto>> GetUserData()
    {
        return await _authenticationAppService.GetUserDataAsync();
    }
}
