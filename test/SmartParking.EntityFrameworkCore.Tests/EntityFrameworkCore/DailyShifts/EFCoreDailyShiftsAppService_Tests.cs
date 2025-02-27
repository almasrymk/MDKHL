using SmartParking.DailyShift;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SmartParking.EntityFrameworkCore.DailyShifts;
[Collection(SmartParkingTestConsts.CollectionDefinitionName)]

public class EFCoreDailyShiftsAppService_Tests : DailyShiftAppService_Tests<SmartParkingEntityFrameworkCoreTestModule>
{
}
