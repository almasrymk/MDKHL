using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartParking.CameraMainTypes;
using SmartParking.Cameras;
using SmartParking.LookupsAppServices;
using Xunit;

namespace SmartParking.EntityFrameworkCore.Applications.CameraMainType;
[Collection(SmartParkingTestConsts.CollectionDefinitionName)]


public class EfCoreCameraMainTypeAppServiceTests : CameraMainTypeAppServiceTests<SmartParkingEntityFrameworkCoreTestModule>
{

}

