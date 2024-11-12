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
	/// abstract method to verfiy otp by phoneNumber 
	/// </summary>
	/// <param name="phoneNumber">phoneNumber of user</param>
	/// <param name="otp">otp code</param>
	/// <returns>boolean</returns>
	public Task<Result<bool>> VerifyOtpAsync(string phoneNumber, string otp);

	/// <summary>
	/// abstract method to forget password otp by userId
	/// </summary>
	/// <param name="userId">userId</param>
	/// <param name="otp">userId</param>
	/// <returns>useriD</returns>
	public Task<Result<ForgetPasswordOtpDto>> ForgetPasswordOtpAsync(Guid userId, string otp);
}
