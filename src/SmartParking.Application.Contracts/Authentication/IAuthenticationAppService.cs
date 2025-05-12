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
    /// Retrieves a <see cref="Tenant"/> entity by its name asynchronously.
    /// </summary>
    /// <param name="phoneNumber">The phone number of the tenant to retrieve.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains 
    /// the <see cref="Tenant"/> entity corresponding to the specified name.
    /// </returns>
    public Task<Tenant> GetTenantByPhoneNumberDtoAsync(string phoneNum);
    /// <summary>
    /// Logs in a user with the provided input.
    /// </summary>
    /// <param name="input">The login input containing username and password.</param>
    /// <returns>A <see cref="LoginResultDto"/> containing the result of the login attempt.</returns>
    Task<Result<LoginResultDto>> LoginAsync(LoginMobileInputDto input);
    /// <summary>
    /// Logs in a user in app with the provided input.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<Result<LoginAppResultDto>> LoginAppAsync(LoginMobileInputDto input);
    /// <summary>
    /// Logs in a user with the provided input.
    /// </summary>
    /// <param name="input">The login input containing username and password.</param>
    /// <returns>A <see cref="IdentityUser"/> containing the result of the login attempt.</returns>
    Task<IdentityUser> LoginForWebAsync(LoginInputDto input);
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
    /// Changes the password for the currently logged-in user.
    /// </summary>
    /// <param name="input">The input containing the current and new passwords.</param>
    /// <returns>A <see cref="BaseResultDto"/> indicating the result of the password change operation.</returns>
    Task<bool> SetNewPasswordWeb(string UserName, string NewPassword);
    Task<UserDataDto> UpdateUser(string phone, string password);
    /// <summary>
    /// Method used to generate toke for user.
    ///Token is 4 numbers.
    /// </summary>
    /// <param name="email">Email for user that needs to generate its token.</param>
    /// <returns>Boolean describes status of process.</returns>
    Task<bool> GenerateOTPForUser(string UserName);

    /// <summary>
    /// Method sends email with otp to user email.
    /// </summary>
    /// <param name="Email">UserEmail</param>
    /// <param name="Otp">Otp</param>
    /// <returns>flag</returns>
    Task<bool> SendSms(string phoneNumber, string message);


    /// <summary>
    /// Method that verifies otp.
    /// </summary>
    /// <param name="Email">Email for user to be register</param>
    /// <param name="OTP">Otp user have written</param>
    /// <returns>flag</returns>

    Task<bool> VerifyOtp(string UserName, string OTP);

    Task<Guid> GetCurrentCustomerId();

    /// <summary>
    /// Generates a JWT token for the specified user ID.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns> The generated JWT token.</returns>
    Task<string> GenerateToken(string userId);

    /// <summary>
    /// Checks if a user with the specified ID exists.
    /// </summary>
    /// <param name="Id"></param>
    /// <returns> The user if found; otherwise, null.</returns>

    Task<IdentityUser?> IsUserFoundAsync(string Id);
    Task<string> GetUserNameByPhoneNumber(string phone);

}
