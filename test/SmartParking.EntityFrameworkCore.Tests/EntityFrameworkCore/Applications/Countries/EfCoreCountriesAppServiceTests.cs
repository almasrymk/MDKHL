using SmartParking.Countries;
using Xunit;

namespace SmartParking.EntityFrameworkCore.Applications.Countries;
[Collection(SmartParkingTestConsts.CollectionDefinitionName)]
public class EfCoreCountriesAppServiceTests : CountryAppServiceTests<SmartParkingEntityFrameworkCoreTestModule>
{
}
