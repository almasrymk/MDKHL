using Volo.Abp.Settings;

namespace SmartParking.Settings;

public class SmartParkingSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(SmartParkingSettings.MySetting1));
    }
}
