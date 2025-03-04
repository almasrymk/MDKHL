using SmartParking.Garages;
using SmartParking.Discounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SmartParking.EntityFrameworkCore.Applications.Discounts;

[Collection(SmartParkingTestConsts.CollectionDefinitionName)]

public class EfCoreDiscountAppService_Tests : DiscountAppService_Tests<SmartParkingEntityFrameworkCoreTestModule>
{

}
