using Autofac.Core;
using SmartParking.CustomerPackages;
using SmartParking.GarageTransactionEndPoints;
using SmartParking.Lookups;
using SmartParking.Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Modularity;
using Xunit;

namespace SmartParking.GarageTransactions;

public abstract class GarageTransactionEndPointsAppServiceTests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
   where TStartupModule : IAbpModule
{
    private readonly IGarageTransactionEndPointsAppService _garageTransactionEndPointsAppService;

    protected GarageTransactionEndPointsAppServiceTests()
    {
        _garageTransactionEndPointsAppService=GetRequiredService<IGarageTransactionEndPointsAppService>();
    }

    [Fact]
    public void ThrowsException_WhenExitTimeIsBeforeEntranceTime()
    {
        var entranceTime = DateTime.Now;
        var exitTime = entranceTime.AddHours(-1);
        var package = new CustomerPackageDto
        {


                Package = new PackageDto
                {

                    PublishFrom = new TimeOnly(8,00), // 8:00 AM
                    PublishTo = new TimeOnly(18,00)  // 6:00 PM
                }
            
        };

        Assert.Throws<BusinessException>(() =>
            _garageTransactionEndPointsAppService.CalculateTimeDependsOnPackage(entranceTime, exitTime, package));
    }

    [Theory]
    [InlineData("2024-11-20T07:00:00", "2024-11-20T09:00:00", 1)] // Before package start
    [InlineData("2024-11-20T19:00:00", "2024-11-20T21:00:00", 3)] // After package end
    [InlineData("2024-11-20T07:00:00", "2024-11-20T21:00:00", 4)] // Both before and after
    [InlineData("2024-11-20T09:00:00", "2024-11-20T17:00:00", 0)] // Fully within package
    public void CalculatesCorrectExtraHours_SameDay(string entrance, string exit, int expectedExtraHours)
    {
        var entranceTime = DateTime.Parse(entrance);
        var exitTime = DateTime.Parse(exit);
        var package = new CustomerPackageDto
        {
            Package = new PackageDto
            {
                PublishFrom = new TimeOnly(8, 00), // 8:00 AM
                PublishTo = new TimeOnly(18, 00)  // 6:00 PM
            }
        };

        var result = _garageTransactionEndPointsAppService.CalculateTimeDependsOnPackage(entranceTime, exitTime, package);

        Assert.Equal(expectedExtraHours, result);
    }

    [Fact]
    public void CalculatesCorrectExtraHours_OvernightStay()
    {
        var entranceTime = new DateTime(2024, 11, 20, 22, 0, 0); // 10:00 PM
        var exitTime = new DateTime(2024, 11, 21, 6, 0, 0); // 6:00 AM next day
        var package = new CustomerPackageDto
        {
            Package = new PackageDto
            {
                PublishFrom = new TimeOnly(8, 00), // 8:00 AM
                PublishTo = new TimeOnly(18, 00)  // 6:00 PM
            }
        };

        var result = _garageTransactionEndPointsAppService.CalculateTimeDependsOnPackage(entranceTime, exitTime, package);

        Assert.Equal(8, result); // 2 hours before 8 AM + 6 hours after 6 PM
    }

    [Fact]
    public void CalculatesCorrectExtraHours_PackageCrossingMidnight()
    {
        var entranceTime = new DateTime(2024, 11, 20, 20, 0, 0); // 8:00 PM
        var exitTime = new DateTime(2024, 11, 21, 4, 0, 0); // 4:00 AM next day
        var package = new CustomerPackageDto
        {
            Package = new PackageDto
            {
                PublishFrom = new TimeOnly(8, 00), // 8:00 AM
                PublishTo = new TimeOnly(18, 00)  // 6:00 PM
            }
        };

        var result = _garageTransactionEndPointsAppService.CalculateTimeDependsOnPackage(entranceTime, exitTime, package);

        Assert.Equal(0, result); // Fully within package time
    }

    [Fact]
    public void CalculatesCorrectExtraHours_MultiDayStay()
    {
        var entranceTime = new DateTime(2024, 11, 20, 7, 0, 0); // 7:00 AM
        var exitTime = new DateTime(2024, 11, 22, 21, 0, 0); // 9:00 PM two days later
        var package = new CustomerPackageDto
        {
            Package = new PackageDto
            {
                PublishFrom = new TimeOnly(8, 00), // 8:00 AM
                PublishTo = new TimeOnly(18, 00)  // 6:00 PM
            }
        };

        var result = _garageTransactionEndPointsAppService.CalculateTimeDependsOnPackage(entranceTime, exitTime, package);

        Assert.Equal(27, result); // 1 hour before + 3 extra nights × 9 hours + 3 hours after
    }


}
