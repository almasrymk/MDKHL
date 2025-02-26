using SmartParking.Garages;
using SmartParking.Shifts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SmartParking.EntityFrameworkCore.Applications.Shifts;

[Collection(SmartParkingTestConsts.CollectionDefinitionName)]

public class EfCoreShiftAppService_Tests : ShiftsAppService_Tests<SmartParkingEntityFrameworkCoreTestModule>
{
}
