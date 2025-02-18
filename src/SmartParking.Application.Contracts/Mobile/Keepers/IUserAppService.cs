
using SmartParking.Results;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Users;

namespace SmartParking.Mobile.Keepers;

public interface IUserAppService : IApplicationService
{

    /// <summary>
    /// Change password for user 
    /// </summary>
    /// <param name="phoneNumber">phoneNumber for user</param>
    /// <param name="password">password of user</param>
    /// <returns>UserDataDto</returns>
    Task<Result<UserDataDto>> UpdateUserAsync(string FullName, string phoneNumber, string password, string email, string image);

    /// <summary>
    /// Retrieves the user data for the currently logged-in user.
    /// </summary>
    /// <returns>A <see cref="UserDataDto"/> containing user information.</returns>
    Task<Result<UserDataDto>> GetUserDataAsync();
}
