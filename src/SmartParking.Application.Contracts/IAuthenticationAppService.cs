using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking;

public class LoginInput
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class LoginResult
{
    public bool Success { get; set; }
    public string AccessToken { get; set; }
    public string ErrorMessage { get; set; }
}

// Service Interface (Contract)

public interface IAuthenticationAppService : IApplicationService
{
    Task<LoginResult> LoginAsync(LoginInput input);
}
