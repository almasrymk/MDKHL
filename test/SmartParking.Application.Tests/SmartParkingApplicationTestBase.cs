using Volo.Abp.Modularity;

namespace SmartParking;

public abstract class SmartParkingApplicationTestBase<TStartupModule> : SmartParkingTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
