using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.Authentication;


// Service Interface (Contract)
public interface IAuthenticationAppService : IApplicationService
{
    Task<LoginResultDto> LoginAsync(LoginInputDto input);
    Task<RegisterResultDto> RegisterAsync(RegisterInputDto input);

}
