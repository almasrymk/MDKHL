using SmartParking.Countries;
using SmartParking.Samples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SmartParking.EntityFrameworkCore.Applications.Countries;
[Collection(SmartParkingTestConsts.CollectionDefinitionName)]
public class EfCoreCountriesAppServiceTests : CountryAppServiceTests<SmartParkingEntityFrameworkCoreTestModule>
{
}
