using System;
using System.Collections.Generic;
using System.Text;
using SmartParking.Localization;
using Volo.Abp.Application.Services;

namespace SmartParking;

/* Inherit your application services from this class.
 */
public abstract class SmartParkingAppService : ApplicationService
{
    protected SmartParkingAppService()
    {
        LocalizationResource = typeof(SmartParkingResource);
    }
}
