using SmartParking.Garages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SmartParking.EntityFrameworkCore.Applications.Garages;

[Collection(SmartParkingTestConsts.CollectionDefinitionName)]
public class EfCoreGarageAppService_Tests : GaragesAppService_Tests<SmartParkingEntityFrameworkCoreTestModule>
{
}
