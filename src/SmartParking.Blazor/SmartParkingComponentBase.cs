using SmartParking.Localization;
using Volo.Abp.AspNetCore.Components;

namespace SmartParking.Blazor;

public abstract class SmartParkingComponentBase : AbpComponentBase
{
    protected SmartParkingComponentBase()
    {
        LocalizationResource = typeof(SmartParkingResource);
    }
}
