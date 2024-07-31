using SmartParking.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace SmartParking.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class SmartParkingController : AbpControllerBase
{
    protected SmartParkingController()
    {
        LocalizationResource = typeof(SmartParkingResource);
    }
}
