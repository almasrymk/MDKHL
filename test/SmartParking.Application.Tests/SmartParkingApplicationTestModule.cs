using Volo.Abp.Modularity;

namespace SmartParking;

[DependsOn(
    typeof(SmartParkingApplicationModule),
    typeof(SmartParkingDomainTestModule)
)]
public class SmartParkingApplicationTestModule : AbpModule
{

}
