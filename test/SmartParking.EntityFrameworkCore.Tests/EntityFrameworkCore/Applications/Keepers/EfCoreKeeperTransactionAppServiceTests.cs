using SmartParking.Keepers;
using Xunit;

namespace SmartParking.EntityFrameworkCore.Applications.Keepers;

[Collection(SmartParkingTestConsts.CollectionDefinitionName)]
public class EfCoreKeeperTransactionAppServiceTests : KeeperTransactionAppServiceTests<SmartParkingEntityFrameworkCoreTestModule>
{
}
