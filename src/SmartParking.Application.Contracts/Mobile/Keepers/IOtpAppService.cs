using SmartParking.Mobile.Otps;
using SmartParking.Results;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.Mobile.Keepers;

public interface IOtpAppService : IApplicationService
{
	/// <summary>
	/// abstract method to generate otp by phoneNumber
	/// </summary>
	/// <param name="phoneNumber">phoneNumber of user</param>
	/// <returns>otp code</returns>
	public Task<Result<string>> GenerateOtpAsync(string phoneNumber);

	/// <summary>
	/// abstract method to generate otp by userId
	/// </summary>
	/// <param name="userId">userId</param>
	/// <returns>otp code</returns>
	public Task<Result<string>> GenerateOtpByUserIdAsync(Guid userId);

    /// <summary>
    /// abstract method to verfiy otp by phoneNumber , otp code
    /// </summary>
    /// <param name="phoneNumber">phoneNumber of user</param>
    /// <param name="otp">otp code</param>
    /// <returns>VerifyOtpDto</returns>
    public Task<Result<VerifyOtpDto>> VerifyOtpAsync(string phoneNumber, string otp);

	/// <summary>
	/// abstract method to forget password otp by phoneNumber
	/// </summary>
	/// <param name="phoneNumber">userId</param>
	/// <returns>Otp code</returns>
	public Task<Result<string>> ForgetPasswordOtpAsync(string phoneNumber);
}
