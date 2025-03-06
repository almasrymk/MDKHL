using SmartParking.DailyShifts;
using SmartParking.Enums;
using SmartParking.KeeperShifts;
using SmartParking.Shifts;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    public async Task KeeperRequestsToEndMidnightShiftInSameDayBeforeMIdnight()
    {
        var tenantId= Guid.NewGuid();
        var StartShiftRequest =
            new CreateUpdateDailyShiftDto
            {
                KeeperId = new Guid("d600ff3d-c859-4327-94de-425f16ec9d55"),
                ShiftId = 1,
                OrganizationId = 1,
                GarageId = 1,
                KeeperShiftRequestType = Enums.KeeperShiftRequestType.StartShift,
                TenantId = tenantId,
                RequestTime = new DateTime(2025, 3, 5, 22, 30, 0)
            };
        var EndShiftRequest =
            new CreateUpdateDailyShiftDto
            {
                KeeperId = new Guid("d600ff3d-c859-4327-94de-425f16ec9d55"),
                ShiftId = 1,
                OrganizationId = 1,
                GarageId = 1,
                KeeperShiftRequestType = Enums.KeeperShiftRequestType.EndShift,
                TenantId = tenantId,
                RequestTime = new DateTime(2025, 3, 6, 23, 0, 0)
            };
        var result = await _dailyShiftAppService.CreateAsync(StartShiftRequest);
        Assert.NotNull(result);
        var exception = await Assert.ThrowsAsync<Exception>(async () => await _dailyShiftAppService.CreateAsync(EndShiftRequest));
        Assert.Equal("Cannot End Shift Now", exception.Message);

    }

    [Fact]
    public async Task KeeperStartMidnightSHiftBeforeMidnightAndEndsAfterMIdnight()
    {
        var tenantId = Guid.NewGuid();

        var StartShiftRequest =
            new CreateUpdateDailyShiftDto
            {
                KeeperId = new Guid("d600ff3d-c859-4327-94de-425f16ec9d55"),
                ShiftId = 1,
                OrganizationId = 1,
                GarageId = 1,
                KeeperShiftRequestType = Enums.KeeperShiftRequestType.StartShift,
                TenantId = tenantId,
                RequestTime = new DateTime(2025, 3, 6, 0, 30, 0)
            };
        var EndShiftRequest =
            new CreateUpdateDailyShiftDto
            {
                KeeperId = new Guid("d600ff3d-c859-4327-94de-425f16ec9d55"),
                ShiftId = 1,
                OrganizationId = 1,
                GarageId = 1,
                KeeperShiftRequestType = Enums.KeeperShiftRequestType.EndShift,
                TenantId = tenantId,
                RequestTime = new DateTime(2025, 3, 6, 1, 30, 0)
            };
        var result = await _dailyShiftAppService.CreateAsync(StartShiftRequest);
        Assert.NotNull(result);
        var exception = await Assert.ThrowsAsync<Exception>(async () => await _dailyShiftAppService.CreateAsync(EndShiftRequest));
        Assert.Equal("Cannot End Shift Now", exception.Message);

    }
    
    [Fact]
    public async Task KeeperRequestsToEndMidnightShiftInSameDayAfterMidnight()
    {
        var tenantId = Guid.NewGuid();

        var StartShiftRequest =
            new CreateUpdateDailyShiftDto
            {
                KeeperId = new Guid("d600ff3d-c859-4327-94de-425f16ec9d55"),
                ShiftId = 1,
                OrganizationId = 1,
                GarageId = 1,
                KeeperShiftRequestType = Enums.KeeperShiftRequestType.StartShift,
                TenantId = tenantId,
                RequestTime = new DateTime(2025, 3, 5, 22, 30, 0)
            };
        var EndShiftRequest =
            new CreateUpdateDailyShiftDto
            {
                KeeperId = new Guid("d600ff3d-c859-4327-94de-425f16ec9d55"),
                ShiftId = 1,
                OrganizationId = 1,
                GarageId = 1,
                KeeperShiftRequestType = Enums.KeeperShiftRequestType.EndShift,
                TenantId = tenantId,
                RequestTime = new DateTime(2025, 3, 6, 1, 30, 0)
            };
        var result = await _dailyShiftAppService.CreateAsync(StartShiftRequest);
        Assert.NotNull(result);
        var exception = await Assert.ThrowsAsync<Exception>(async () => await _dailyShiftAppService.CreateAsync(EndShiftRequest));
        Assert.Equal("Cannot End Shift Now", exception.Message);

    }
    
    [Fact]
    public async Task KeeperEndsShiftSuccessfullyWhenAnotherKeeperIsInSameShiftbeforemidnight()
    {
        var tenantId = Guid.NewGuid();

        var StartShiftRequest =
            new CreateUpdateDailyShiftDto
            {
                KeeperId = new Guid("d600ff3d-c859-4327-94de-425f16ec9d55"),
                ShiftId = 1,
                OrganizationId = 1,
                GarageId = 1,
                TenantId = tenantId,
                KeeperShiftRequestType = Enums.KeeperShiftRequestType.StartShift,
                RequestTime = new DateTime(2025, 3, 5, 22, 30, 0)
            };
        var StartShiftRequest2 =
            new CreateUpdateDailyShiftDto
            {
                KeeperId = new Guid("d600ff3d-c859-4327-94de-425f16ec9d54"),
                ShiftId = 1,
                OrganizationId = 1,
                GarageId = 1,
                TenantId = tenantId,
                KeeperShiftRequestType = Enums.KeeperShiftRequestType.StartShift,
                RequestTime = new DateTime(2025, 3, 5, 22, 40, 0)
            };
        var EndShiftRequest =
            new CreateUpdateDailyShiftDto
            {
                KeeperId = new Guid("d600ff3d-c859-4327-94de-425f16ec9d55"),
                ShiftId = 1,
                OrganizationId = 1,
                GarageId = 1,
                TenantId=tenantId,
                KeeperShiftRequestType = Enums.KeeperShiftRequestType.EndShift,
                RequestTime = new DateTime(2025, 3, 6, 1, 30, 0)
            };
        await _dailyShiftAppService.CreateAsync(StartShiftRequest);
        await _dailyShiftAppService.CreateAsync(StartShiftRequest2);
        var result2= await _dailyShiftAppService.CreateAsync(EndShiftRequest);
        Assert.NotNull(result2);
    }
    
    [Fact]
    public async Task KeeperEndsShiftSuccessfullyWhenAnotherKeeperIsInSameShiftAftermidnight()
    {
        var tenantId = Guid.NewGuid();

        var StartShiftRequest =
            new CreateUpdateDailyShiftDto
            {
                KeeperId = new Guid("d600ff3d-c859-4327-94de-425f16ec9d55"),
                ShiftId = 1,
                OrganizationId = 1,
                GarageId = 1,
                TenantId = tenantId,
                KeeperShiftRequestType = Enums.KeeperShiftRequestType.StartShift,
                RequestTime = new DateTime(2025, 3, 5, 22, 30, 0)
            };
        var StartShiftRequest2 =
            new CreateUpdateDailyShiftDto
            {
                KeeperId = new Guid("d600ff3d-c859-4327-94de-425f16ec9d54"),
                ShiftId = 1,
                OrganizationId = 1,
                GarageId = 1,
                TenantId = tenantId,
                KeeperShiftRequestType = Enums.KeeperShiftRequestType.StartShift,
                RequestTime = new DateTime(2025, 3, 6, 1, 0, 0)
            };
        var EndShiftRequest =
            new CreateUpdateDailyShiftDto
            {
                KeeperId = new Guid("d600ff3d-c859-4327-94de-425f16ec9d55"),
                ShiftId = 1,
                OrganizationId = 1,
                GarageId = 1,
                TenantId=tenantId,
                KeeperShiftRequestType = Enums.KeeperShiftRequestType.EndShift,
                RequestTime = new DateTime(2025, 3, 6, 1, 30, 0)
            };
        await _dailyShiftAppService.CreateAsync(StartShiftRequest);
        await _dailyShiftAppService.CreateAsync(StartShiftRequest2);
        var result2= await _dailyShiftAppService.CreateAsync(EndShiftRequest);
        Assert.NotNull(result2);
    }
    
    [Fact]
    public async Task KeeperEndsShiftSuccessfullyWhenAnotherKeeperIsInSameDay()
    {
        var tenantId = Guid.NewGuid();

        var StartShiftRequest =
            new CreateUpdateDailyShiftDto
            {
                KeeperId = new Guid("d600ff3d-c859-4327-94de-425f16ec9d55"),
                ShiftId = 2,
                OrganizationId = 1,
                GarageId = 1,
                TenantId = tenantId,
                KeeperShiftRequestType = Enums.KeeperShiftRequestType.StartShift,
                RequestTime = new DateTime(2025, 3, 5, 11, 0, 0)
            };
        var StartShiftRequest2 =
            new CreateUpdateDailyShiftDto
            {
                KeeperId = new Guid("d600ff3d-c859-4327-94de-425f16ec9d54"),
                ShiftId = 2,
                OrganizationId = 1,
                GarageId = 1,
                TenantId = tenantId,
                KeeperShiftRequestType = Enums.KeeperShiftRequestType.StartShift,
                RequestTime = new DateTime(2025, 3, 5, 11, 30, 0)
            };
        var EndShiftRequest =
            new CreateUpdateDailyShiftDto
            {
                KeeperId = new Guid("d600ff3d-c859-4327-94de-425f16ec9d55"),
                ShiftId = 2,
                OrganizationId = 1,
                GarageId = 1,
                TenantId=tenantId,
                KeeperShiftRequestType = Enums.KeeperShiftRequestType.EndShift,
                RequestTime = new DateTime(2025, 3, 5, 12, 30, 0)
            };
        await _dailyShiftAppService.CreateAsync(StartShiftRequest);
        await _dailyShiftAppService.CreateAsync(StartShiftRequest2);
        var result2= await _dailyShiftAppService.CreateAsync(EndShiftRequest);
        Assert.NotNull(result2);
    }

}
