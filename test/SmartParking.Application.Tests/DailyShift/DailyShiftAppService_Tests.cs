using SmartParking.DailyShifts;
using SmartParking.Enums;
using SmartParking.KeeperShifts;
using SmartParking.Shifts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Modularity;
using Xunit;

namespace SmartParking.DailyShift;

public abstract class DailyShiftAppService_Tests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IShiftAppService _shiftAppService;
    private readonly IKeeperShiftsAppService _keeperShiftsAppService;
    private readonly IDailyShiftAppService _dailyShiftAppService;

    protected DailyShiftAppService_Tests()
    {
        _keeperShiftsAppService = GetRequiredService<IKeeperShiftsAppService>();
        _shiftAppService = GetRequiredService<IShiftAppService>();
        _dailyShiftAppService = GetRequiredService<IDailyShiftAppService>();
    }
    [Fact]
    public async Task KeeperRequestsToEndShiftInValidTimeSameDay()
    {
        var tenantId = Guid.NewGuid();

        // Arrange
        var shift1 = new CreateUpdateShiftDto
        {
            Description = "Morning Shift",
            StartDate = new DateOnly(2023, 10, 1),
            EndDate = new DateOnly(2023, 10, 12),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(12, 0),
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Active,
            TenantId=tenantId,
            ArabicName = "زعتر",
            EnglishName = "en",
            IsFullDay = false,
            Notes = ""
        };
        var shift = await _shiftAppService.CreateAsync(shift1);
        var keeperId = Guid.NewGuid();
        var keepershift = new CreateUpdateKeeperShiftDto
        {
            GarageId = 1,
            OrganizationId = 1,
            KeeperId = keeperId,
            ShiftId = shift.Id,
            StartDate = new DateOnly(2023, 10, 2),
            TenantId = tenantId,
            EndDate = new DateOnly(2023, 10, 5)
        };
        await _keeperShiftsAppService.CreateAsync(keepershift);
        var keeperShiftRequestStart = new CreateUpdateDailyShiftDto
        {
            KeeperId = keeperId, 
            OrganizationId = 1, 
            GarageId = 1, 
            ShiftId = shift.Id, 
            TenantId= tenantId,
            RequestTime = DateTime.Today.AddHours(8), 
            KeeperShiftRequestType = KeeperShiftRequestType.StartShift 
        };
        var keeperShiftRequestEnd = new CreateUpdateDailyShiftDto
        {
            KeeperId = keeperId,
            OrganizationId = 1,
            GarageId = 1,
            ShiftId = 1,
            TenantId = tenantId,
            RequestTime = DateTime.Today.AddHours(13),

            KeeperShiftRequestType = KeeperShiftRequestType.EndShift
        };
        await _dailyShiftAppService.CreateAsync(keeperShiftRequestStart);
        var exception = await Assert.ThrowsAsync<Exception>(async () => await _dailyShiftAppService.CreateAsync(keeperShiftRequestStart));
        Assert.Equal("CannotEndShift", exception.Message);

    }

}
