using Xunit;

namespace SmartParking.EntityFrameworkCore;

[CollectionDefinition(SmartParkingTestConsts.CollectionDefinitionName)]
public class SmartParkingEntityFrameworkCoreCollection : ICollectionFixture<SmartParkingEntityFrameworkCoreFixture>
{

}
