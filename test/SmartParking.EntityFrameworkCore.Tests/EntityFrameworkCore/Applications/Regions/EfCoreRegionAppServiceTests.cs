
using SmartParking.Regions;
using Xunit;

namespace SmartParking.EntityFrameworkCore.Applications.Regions;
[Collection(SmartParkingTestConsts.CollectionDefinitionName)]

public class EfCoreRegionAppServiceTests : RegionAppServiceTests<SmartParkingEntityFrameworkCoreTestModule>
{
}
