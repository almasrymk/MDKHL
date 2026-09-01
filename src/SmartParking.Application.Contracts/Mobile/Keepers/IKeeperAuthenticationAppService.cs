using SmartParking.Results;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.Mobile.Keepers;

public interface IKeeperAuthenticationAppService : IApplicationService
{
    /// <summary>
    /// Authenticates a keeper user and issues a JWT token with session metadata.
    /// </summary>
    /// <param name="input">The login input containing username/phone and password.</param>
    /// <returns>A <see cref="LoginResponseDto"/> containing the result of the login attempt.</returns>
    Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto input);
}
