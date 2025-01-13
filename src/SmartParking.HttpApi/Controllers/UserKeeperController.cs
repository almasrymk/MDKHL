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

    [HttpPost("setNewPassword")]
    [Authorize]
    public async Task<Result> SetNewPassword(ChangePasswordDto input)
    {
        return await _userAppService.SetNewPasswordAsync(input);
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
