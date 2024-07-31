using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace SmartParking.Blazor.Client;

[Dependency(ReplaceServices = true)]
public class SmartParkingBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "SmartParking";
}
