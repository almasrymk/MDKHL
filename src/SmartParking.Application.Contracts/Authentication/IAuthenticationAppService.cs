using IdentityUser = Volo.Abp.Identity.IdentityUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.TenantManagement;

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
    /// Logs in a user with the provided input.
    /// </summary>
    /// <param name="input">The login input containing username and password.</param>
    /// <returns>A <see cref="LoginResultDto"/> containing the result of the login attempt.</returns>
    Task<LoginResultDto> LoginAsync(LoginInputDto input);
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
    Task<RegisterResultDto> RegisterAsync(RegisterInputDto input);

    /// <summary>
    /// Retrieves the user data for the currently logged-in user.
    /// </summary>
    /// <returns>A <see cref="UserDataDto"/> containing user information.</returns>
    Task<UserDataDto> GetUserDataAsync();

    /// <summary>
    /// Initiates the password reset process for the user identified by phone number.
    /// </summary>
    /// <param name="phone">The phone number associated with the user account.</param>
    /// <returns>A <see cref="forgetpasswordDto"/> containing the result of the password reset initiation.</returns>
    Task<forgetpasswordDto> ForgetPasswordAsync(string phone);

    /// <summary>
    /// Changes the password for the currently logged-in user.
    /// </summary>
    /// <param name="input">The input containing the current and new passwords.</param>
    /// <returns>A <see cref="BaseResultDto"/> indicating the result of the password change operation.</returns>
    Task<BaseResultDto> ChangePassword(ChangePasswordInputDto input);
}
