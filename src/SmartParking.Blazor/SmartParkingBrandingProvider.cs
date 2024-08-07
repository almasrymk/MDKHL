using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace SmartParking.Blazor;

[Dependency(ReplaceServices = true)]
public class SmartParkingBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "SmartParking";
}
