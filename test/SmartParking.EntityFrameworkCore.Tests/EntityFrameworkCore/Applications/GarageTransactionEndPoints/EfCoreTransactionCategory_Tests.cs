using SmartParking.GarageTransactionEndPoints;
using Xunit;

namespace SmartParking.EntityFrameworkCore.Applications.GarageTransactionEndPoints;

[Collection(SmartParkingTestConsts.CollectionDefinitionName)]
public class EfCoreTransactionCategory_Tests : TransactionCategory_Tests<SmartParkingEntityFrameworkCoreTestModule>
{
}
