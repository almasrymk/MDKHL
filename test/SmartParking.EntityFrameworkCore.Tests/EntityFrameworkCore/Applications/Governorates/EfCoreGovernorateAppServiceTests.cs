using SmartParking.Governorates;
using Xunit;


namespace SmartParking.EntityFrameworkCore.Applications.Governorates;
[Collection(SmartParkingTestConsts.CollectionDefinitionName)]

public class EfCoreGovernorateAppServiceTests : GovernorateAppServiceTests<SmartParkingEntityFrameworkCoreTestModule>
{
}
