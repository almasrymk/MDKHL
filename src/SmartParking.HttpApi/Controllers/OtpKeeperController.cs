using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Mobile.Keepers;
using SmartParking.Results;
using System;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;

namespace SmartParking.Controllers;

[Route("api/Keeper/Otp")]
[ApiController]

public class OtpKeeperController : AbpController
{
    private readonly IOtpAppService _otpAppService;
	public OtpKeeperController(IOtpAppService otpAppService)
    {
        _otpAppService = otpAppService;
    }

    [HttpPost("generateOtp")]
	[AllowAnonymous]
	[IgnoreAntiforgeryToken]
	public async Task<Result<string>> GenerateOtp([FromQuery] string phoneNumber)
	{
		return await _otpAppService.GenerateOtpAsync(phoneNumber);
	}

	[HttpPost("verfiyOtp")]
	[Authorize]
	public async Task<Result<bool>> VerfiyOtp([FromQuery] string phoneNumber, string otp)
	{
		return await _otpAppService.VerifyOtpAsync(phoneNumber, otp);
	}

	[HttpPost("requestOtp")]
	[Authorize]
	public async Task<Result<string>> RequestOtp([FromQuery] Guid userId)
	{
		return await _otpAppService.GenerateOtpByUserIdAsync(userId);
	}

	[HttpPost("forgetPasswordOtp")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<Result<ForgetPasswordOtpDto>> ForgetPasswordOtp([FromQuery] Guid userId, string otp)
	{
		return await _otpAppService.ForgetPasswordOtpAsync(userId, otp);
	}
}
