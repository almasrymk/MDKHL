using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Linq;

namespace SmartParking.Blazor;

[Route("api/[controller]")]
[ApiController]
public class SetLanguage : ControllerBase
{
    [HttpGet]
    public IActionResult SetLanguages(string returnUrl)
    {
        IRequestCultureFeature culture = HttpContext.Features.Get<IRequestCultureFeature>();
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(
                new RequestCulture(new string[] { "en", "ar" }
                .Where(option => option != culture.RequestCulture.Culture.Name)
                .FirstOrDefault(), new string[] { "en", "ar" }
                .Where(option => option != culture.RequestCulture.Culture.Name)
        .FirstOrDefault())));
       
        return Redirect(returnUrl);
    }
}
