using System.Threading.Tasks;

namespace SmartParking.Data;

public interface ISmartParkingDbSchemaMigrator
{
    Task MigrateAsync();
}
