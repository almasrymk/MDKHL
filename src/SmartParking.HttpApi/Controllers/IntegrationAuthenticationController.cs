using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Authentication;
using SmartParking.Mobile.Keepers;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Controllers;
[Route("api/[controller]")]
[ApiController]
public class IntegrationAuthenticationController : ControllerBase
{
    private readonly IAuthenticationAppService _authenticationAppService;

    public IntegrationAuthenticationController(IAuthenticationAppService authenticationAppService)
    {
        _authenticationAppService = authenticationAppService;
    }


    [HttpPost("login")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]

    public async Task<Result<BaseLoginResponse>> Login(LoginMobileInputDto input)
    {
        try
        {
            var _loginResponse =await _authenticationAppService.LoginAsync(input);
            if(_loginResponse.ValidationErrors.Count > 0)
            {
                return Result<BaseLoginResponse>.Invalid(_loginResponse.ValidationErrors);
            }
            return Result<BaseLoginResponse>.Success(new BaseLoginResponse
            {
                AccessToken = _loginResponse.Value.AccessToken
            });
        }
        catch
        {
            return Result<BaseLoginResponse>.Invalid(new Results.ValidationError
            {
                ErrorCode = "403",
                ErrorMessage = "This user not found",
            });
        }
    }

}
