using SmartParking.Organizations;
using SmartParking.Samples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SmartParking.EntityFrameworkCore.Applications.Organizations;

[Collection(SmartParkingTestConsts.CollectionDefinitionName)]
public class EfCoreOrganizationAppServiceTests : OrganizationAppServiceTests<SmartParkingEntityFrameworkCoreTestModule>
{
}
