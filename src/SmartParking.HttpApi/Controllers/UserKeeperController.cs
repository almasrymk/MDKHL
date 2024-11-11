using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Mobile.Keepers;
using SmartParking.Results;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;

namespace SmartParking.Controllers;

[Route("api/Keeper/User")]
[ApiController]
public class UserKeeperController : AbpController
{
    private readonly IUserAppService _userAppService;
    public UserKeeperController(IUserAppService userAppService)
    {
        _userAppService = userAppService;

    }

    [HttpPost("changePassword")]
    [Authorize]
    public async Task<Result> ChangePassword(ChangePasswordDto input)
    {
        return await _userAppService.ChangePasswordAsync(input);
    }

    [HttpPost("profileUser")]
    [Authorize]
    public async Task<Result<UserDataDto>> UpdateProfileUser(string phoneNumber, string password)
    {
        return await _userAppService.UpdateUserAsync(phoneNumber, password);
    }

    [HttpGet]
    [Authorize]
    public async Task<Result<UserDataDto>> GetUserData()
    {
        return await _userAppService.GetUserDataAsync();
    }
}
