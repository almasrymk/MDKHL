using SmartParking.Shifts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SmartParking.EntityFrameworkCore.Applications.DailyShifts;
[Collection(SmartParkingTestConsts.CollectionDefinitionName)]
public class EFCoreDailyShiftsAppService_Tests : DailyShiftsAppService_Tests<SmartParkingEntityFrameworkCoreTestModule>
{
}

