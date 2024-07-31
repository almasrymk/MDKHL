using Volo.Abp.Modularity;

namespace SmartParking;

[DependsOn(
    typeof(SmartParkingDomainModule),
    typeof(SmartParkingTestBaseModule)
)]
public class SmartParkingDomainTestModule : AbpModule
{

}
