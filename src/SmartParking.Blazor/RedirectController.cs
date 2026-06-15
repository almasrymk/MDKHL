using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Route = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace SmartParking.Blazor;

[RemoteService(false)]
[ApiController]
[Route("api/[controller]")]
public class RedirectController : ControllerBase
{

    private readonly NavigationManager navigationManager;

    public RedirectController(NavigationManager navigationManager)
    {
        this.navigationManager = navigationManager;
    }

    [HttpGet("checkout-redirect")]
    public IActionResult CheckoutExtensionRedirect()
    {
        return Redirect($"/checkout-extension");
    }
}
