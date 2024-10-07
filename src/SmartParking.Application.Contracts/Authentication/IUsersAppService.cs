
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.Authentication;

public interface IUsersAppService:  IApplicationService
{
    public Task<UserDto> CreateAsync(UserCreateDto input);
   
}
