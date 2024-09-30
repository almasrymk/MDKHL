
using SmartParking.States;
using Xunit;

namespace SmartParking.EntityFrameworkCore.Applications.States;
[Collection(SmartParkingTestConsts.CollectionDefinitionName)]

public class EfCoreStateAppServiceTests : StateAppServiceTests<SmartParkingEntityFrameworkCoreTestModule>
{
}
