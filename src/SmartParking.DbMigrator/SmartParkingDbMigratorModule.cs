using SmartParking.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace SmartParking.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(SmartParkingEntityFrameworkCoreModule),
    typeof(SmartParkingApplicationContractsModule)
    )]
public class SmartParkingDbMigratorModule : AbpModule
{
}
