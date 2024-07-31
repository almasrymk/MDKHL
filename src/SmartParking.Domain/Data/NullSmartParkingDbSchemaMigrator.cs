using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SmartParking.Data;

/* This is used if database provider does't define
 * ISmartParkingDbSchemaMigrator implementation.
 */
public class NullSmartParkingDbSchemaMigrator : ISmartParkingDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
