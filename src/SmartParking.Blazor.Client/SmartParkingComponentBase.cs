using SmartParking.Localization;
using Volo.Abp.AspNetCore.Components;

namespace SmartParking.Blazor.Client;

public abstract class SmartParkingComponentBase : AbpComponentBase
{
    protected SmartParkingComponentBase()
    {
        LocalizationResource = typeof(SmartParkingResource);
    }
}
