using SmartParking.PackageTypes;
using Xunit;

namespace SmartParking.EntityFrameworkCore.Applications.PackageTypes;
[Collection(SmartParkingTestConsts.CollectionDefinitionName)]

public class EfCorePackageTypeAppServiceTests : PackageTypeAppServiceTests<SmartParkingEntityFrameworkCoreTestModule>
{
}
