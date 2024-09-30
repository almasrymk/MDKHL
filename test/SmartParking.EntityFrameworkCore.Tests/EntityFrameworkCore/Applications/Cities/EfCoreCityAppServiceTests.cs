using SmartParking.Cities;
using Xunit;

namespace SmartParking.EntityFrameworkCore.Applications.Cities;
[Collection(SmartParkingTestConsts.CollectionDefinitionName)]

public class EfCoreCityAppServiceTests : CityAppServiceTests<SmartParkingEntityFrameworkCoreTestModule>
{
}
