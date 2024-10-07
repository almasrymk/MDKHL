using SmartParking.Cameras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SmartParking.EntityFrameworkCore.Applications.Cameras;
[Collection(SmartParkingTestConsts.CollectionDefinitionName)]

public class EfCoreCamerasAppServiceTests : CameraAppServiceTests<SmartParkingEntityFrameworkCoreTestModule>
{
}
