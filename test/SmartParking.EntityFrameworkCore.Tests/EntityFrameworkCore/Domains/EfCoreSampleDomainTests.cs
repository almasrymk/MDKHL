using SmartParking.Samples;
using Xunit;

namespace SmartParking.EntityFrameworkCore.Domains;

[Collection(SmartParkingTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<SmartParkingEntityFrameworkCoreTestModule>
{

}
