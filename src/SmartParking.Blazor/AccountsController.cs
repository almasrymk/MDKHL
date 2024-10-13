using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Authentication;
using Microsoft.AspNetCore.Authentication;

namespace SmartParking.Blazor;
[Route("api/[controller]")]
[ApiController]
public class AccountsController : ControllerBase
{
    private readonly IAuthenticationAppService _authenticationApp;

    public AccountsController(
        IAuthenticationAppService authenticationApp
        )
    {
        _authenticationApp = authenticationApp;
    }

    [HttpGet]
    public async Task<IActionResult> Login(string username , string password)
    {


        LoginInputDto loginInputDto = new LoginInputDto()
        {
            Username = username,
            Password = password
        };
       var userLogin= await _authenticationApp.LoginForWebAsync(loginInputDto);
        if (userLogin is not null)
        {
            return LocalRedirect("/");
            
        }
        
        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return LocalRedirect("/login");
        
    }

    [HttpGet("/logOut")]
    public async Task<ActionResult> Logout()
    {
        await _authenticationApp.LogOutAsync();
        return LocalRedirect("/login");
    }


}


