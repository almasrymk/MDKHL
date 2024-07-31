using Volo.Abp.Modularity;

namespace SmartParking;

/* Inherit from this class for your domain layer tests. */
public abstract class SmartParkingDomainTestBase<TStartupModule> : SmartParkingTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
