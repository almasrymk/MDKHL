using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Authentication;
using SmartParking.Mobile.Keepers;
using SmartParking.Results;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using UserDataDto = SmartParking.Mobile.Keepers.UserDataDto;
namespace SmartParking.Controllers;

[Route("api/Keeper/User")]
[ApiController]
public class UserKeeperController : AbpController
{
    private readonly IUserAppService _userAppService;
    private readonly IAuthenticationAppService _authenticationAppService;

    public UserKeeperController(IUserAppService userAppService, IAuthenticationAppService authenticationAppService)
    {
        _userAppService = userAppService;
        this._authenticationAppService = authenticationAppService;
    }

    [HttpPost("setNewPassword")]
    [Authorize]
    public async Task<Result> SetNewPassword(ChangePasswordInputDto input)
    {
        return await _authenticationAppService.SetNewPassword(input);
    }

    [HttpPost("profileUser")]
    [Authorize]
    public async Task<Result<UserDataDto>>UpdateUserAsync(string ?FullName, string? phoneNumber, string? password, string? email, string? image)
    {
        return await _userAppService.UpdateUserAsync(FullName,phoneNumber, password, email, image);
    }

    [HttpGet]
    [Authorize]
    public async Task<Result<UserDataDto>> GetUserData()
    {
        return await _userAppService.GetUserDataAsync();
    }
}
