using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace SmartParking.Blazor;

[Route("api/[controller]")]
[ApiController]
public class SetLanguage : ControllerBase
{
    [HttpPost]
    public IActionResult SetLanguages(string culture, string returnUrl)
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
        );

        return Redirect("/organizations");
    }
}
