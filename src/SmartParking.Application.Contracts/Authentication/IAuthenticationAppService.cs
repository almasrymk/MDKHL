using IdentityUser = Volo.Abp.Identity.IdentityUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.TenantManagement;
using SmartParking.Results;

namespace SmartParking.Authentication;


/// <summary>
/// Interface for authentication-related operations.
/// </summary>
public interface IAuthenticationAppService : IApplicationService
{
    /// <summary>
    /// Retrieves a <see cref="Tenant"/> entity by its name asynchronously.
    /// </summary>
    /// <param name="name">The name of the tenant to retrieve.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains 
    /// the <see cref="Tenant"/> entity corresponding to the specified name.
    /// </returns>
    public Task<Tenant> GetTenantByNameDtoAsync(string name);

    /// <summary>
    /// Retrieves a <see cref="Tenant"/> entity by its phone number asynchronously.
    /// </summary>
    /// <param name="phoneNumber">The phone number of the tenant to retrieve.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains 
    /// the <see cref="Tenant"/> entity corresponding to the specified phone number.
    /// </returns>
    public Task<Tenant> GetTenantByPhoneNumberDtoAsync(string phoneNum);

    /// <summary>
    /// Logs in a user with the provided input.
    /// </summary>
    /// <param name="input">The login input containing username and password.</param>
    /// <returns>A <see cref="LoginResultDto"/> containing the result of the login attempt.</returns>
    Task<Result<LoginResultDto>> LoginAsync(LoginMobileInputDto input);

    /// <summary>
    /// Logs in a user in the app with the provided input.
    /// </summary>
    /// <param name="input">The login input containing username and password.</param>
    /// <returns>A <see cref="LoginAppResultDto"/> containing the result of the login attempt.</returns>
    Task<Result<LoginAppResultDto>> LoginAppAsync(LoginMobileInputDto input);

    /// <summary>
    /// Logs in a user with the provided input for the web platform.
    /// </summary>
    /// <param name="input">The login input containing username and password.</param>
    /// <returns>A <see cref="IdentityUser"/> containing the result of the login attempt.</returns>
    Task<IdentityUser> LoginForWebAsync(LoginInputDto input);

    /// <summary>
    /// Logs out the currently logged-in user.
    /// </summary>
    /// <returns>A task representing the asynchronous logout operation.</returns>
    Task LogOutAsync();

    /// <summary>
    /// Registers a new user with the provided input.
    /// </summary>
    /// <param name="input">The registration input containing user details.</param>
    /// <returns>A <see cref="RegisterResultDto"/> containing the result of the registration attempt.</returns>
    Task<Result<RegisterResultDto>> RegisterAsync(RegisterInputDto input);

    /// <summary>
    /// Retrieves the user data for the currently logged-in user.
    /// </summary>
    /// <returns>A <see cref="UserDataDto"/> containing user information.</returns>
    Task<Result<UserDataDto>> GetUserDataAsync();

    /// <summary>
    /// Initiates the password reset process for the user identified by phone number.
    /// </summary>
    /// <param name="phone">The phone number associated with the user account.</param>
    /// <returns>A <see cref="forgetpasswordDto"/> containing the result of the password reset initiation.</returns>
    Task<Result<forgetpasswordDto>> ForgetPasswordAsync(string phone);

    /// <summary>
    /// Changes the password for the currently logged-in user.
    /// </summary>
    /// <param name="input">The input containing the current and new passwords.</param>
    /// <returns>A <see cref="BaseResultDto"/> indicating the result of the password change operation.</returns>
    Task<Result> SetNewPassword(ChangePasswordInputDto input);

    /// <summary>
    /// Changes the password for the currently logged-in user on the web platform.
    /// </summary>
    /// <param name="UserName">The username of the user.</param>
    /// <param name="NewPassword">The new password to set.</param>
    /// <returns>A boolean indicating whether the password change was successful.</returns>
    Task<bool> SetNewPasswordWeb(string UserName, string NewPassword);

    /// <summary>
    /// Updates the user data with the provided phone number and password.
    /// </summary>
    /// <param name="phone">The phone number of the user.</param>
    /// <param name="password">The new password for the user.</param>
    /// <returns>A <see cref="UserDataDto"/> representing the updated user data.</returns>
    Task<UserDataDto> UpdateUser(string phone, string password);

    /// <summary>
    /// Generates a 4-digit OTP for the user.
    /// </summary>
    /// <param name="UserName">The username for the user to generate the OTP.</param>
    /// <returns>A boolean indicating the success of the OTP generation process.</returns>
    Task<bool> GenerateOTPForUser(string UserName);

    /// <summary>
    /// Sends an SMS with an OTP to the provided phone number.
    /// </summary>
    /// <param name="phoneNumber">The phone number to send the OTP to.</param>
    /// <param name="message">The message containing the OTP to send.</param>
    /// <returns>A boolean indicating the success of the SMS sending process.</returns>
    Task<bool> SendSms(string phoneNumber, string message);

    /// <summary>
    /// Verifies the provided OTP for the user.
    /// </summary>
    /// <param name="UserName">The username of the user to verify the OTP for.</param>
    /// <param name="OTP">The OTP provided by the user.</param>
    /// <returns>A boolean indicating whether the OTP verification was successful.</returns>
    Task<bool> VerifyOtp(string UserName, string OTP);

    /// <summary>
    /// Retrieves the current customer ID for the logged-in user.
    /// </summary>
    /// <returns>The customer ID associated with the current user.</returns>
    Task<Guid> GetCurrentCustomerId();

    /// <summary>
    /// Generates a JWT token for the specified user ID.
    /// </summary>
    /// <param name="userId">The user ID to generate the token for.</param>
    /// <returns>The generated JWT token.</returns>
    Task<string> GenerateToken(string userId);

    /// <summary>
    /// Checks if a user with the specified ID exists.
    /// </summary>
    /// <param name="Id">The ID of the user to check for existence.</param>
    /// <returns>The <see cref="IdentityUser"/> if found; otherwise, null.</returns>
    Task<IdentityUser?> IsUserFoundAsync(string Id);

    /// <summary>
    /// Retrieves the username associated with the specified phone number.
    /// </summary>
    /// <param name="phone">The phone number to search for the username.</param>
    /// <returns>The username associated with the phone number.</returns>
    Task<string> GetUserNameByPhoneNumber(string phone);

}
