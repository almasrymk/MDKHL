using SmartParking.Users;
using System;
using Xunit;

namespace SmartParking.EntityFrameworkCore.Applications.Users;

[Collection(SmartParkingTestConsts.CollectionDefinitionName)]
public class EfCoreUserAppServiceTests : UserAppServiceTests<SmartParkingEntityFrameworkCoreTestModule>
{
}
