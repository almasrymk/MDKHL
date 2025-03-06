using AutoMapper.Internal.Mappers;
using Shouldly;
using SmartParking.DailyShifts;
using SmartParking.Entities.Shifts;
using SmartParking.KeeperShifts;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace SmartParking.Shifts;

public abstract class DailyShiftsAppService_Tests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IDailyShiftAppService _dailyShiftAppService;
    protected DailyShiftsAppService_Tests()
    {
        _dailyShiftAppService = GetRequiredService<IDailyShiftAppService>();
    }
    [Fact]
    public async Task Should_Start_Shift_Successfully()
    {
        var dailyShift = await _dailyShiftAppService.CreateAsync(
            new CreateUpdateDailyShiftDto
            {
                KeeperId = new Guid("d600ff3d-c859-4327-94de-425f16ec9d55"),
                ShiftId = 2,
                OrganizationId = 1,
                GarageId = 1,
                KeeperShiftRequestType = Enums.KeeperShiftRequestType.StartShift,
                TenantId = new Guid(),
                RequestTime = new DateTime(2025, 3, 5, 11, 30, 0)
            }
            );
        Assert.NotNull(dailyShift);
    }
    [Fact]
    public async Task Should_Connot_Start_Shift_OutOfRenageDate()
    {
        var createUpdateDailyShift =
            new CreateUpdateDailyShiftDto
            {
                KeeperId = new Guid("d600ff3d-c859-4327-94de-425f16ec9d55"),
                ShiftId = 3,
                OrganizationId = 1,
                GarageId = 1,
                KeeperShiftRequestType = Enums.KeeperShiftRequestType.StartShift,
                TenantId = new Guid(),
                RequestTime = new DateTime(2025, 3, 5, 11, 30, 0)
            };
            
        await Assert.ThrowsAsync<Exception>(() => _dailyShiftAppService.CreateAsync(createUpdateDailyShift));
    }
    [Fact]
    public async Task Should_Connot_Start_Shift_OutOfRenageTime()
    {
        var createUpdateDailyShift =
            new CreateUpdateDailyShiftDto
            {
                KeeperId = new Guid("d600ff3d-c859-4327-94de-425f16ec9d55"),
                ShiftId =2 ,
                OrganizationId = 1,
                GarageId = 1,
                KeeperShiftRequestType = Enums.KeeperShiftRequestType.StartShift,
                TenantId = new Guid(),
                RequestTime = new DateTime(2025, 3, 5, 9, 30, 0)
            };

        await Assert.ThrowsAsync<Exception>(() => _dailyShiftAppService.CreateAsync(createUpdateDailyShift));
    }
    /// Mid Night Shift 
    /// Start Time : 22 : 30 
    /// End Time : 2 : 30 
    /// should start shift within this range only 
    [Fact]
    public async Task Should_Start_MidNightShift_Successfully()
    {
        var createUpdateDailyShift =
           new CreateUpdateDailyShiftDto
           {
               KeeperId = new Guid("d600ff3d-c859-4327-94de-425f16ec9d55"),
               ShiftId = 1,
               OrganizationId = 1,
               GarageId = 1,
               KeeperShiftRequestType = Enums.KeeperShiftRequestType.StartShift,
               TenantId = new Guid(),
               RequestTime = new DateTime(2025, 3, 5, 23, 30, 0)
           };
        var result = await _dailyShiftAppService.CreateAsync(createUpdateDailyShift);
        Assert.NotNull(result);
    }
    [Fact]
    public async Task Should_Start_MidNightShift_Successfully_2()
    {
        var createUpdateDailyShift =
           new CreateUpdateDailyShiftDto
           {
               KeeperId = new Guid("d600ff3d-c859-4327-94de-425f16ec9d55"),
               ShiftId = 1,
               OrganizationId = 1,
               GarageId = 1,
               KeeperShiftRequestType = Enums.KeeperShiftRequestType.StartShift,
               TenantId = new Guid(),
               RequestTime = new DateTime(2025, 3, 6, 0, 30, 0)
           };
        var result = await _dailyShiftAppService.CreateAsync(createUpdateDailyShift);
        Assert.NotNull(result);
    }
    [Fact]
    public async Task Should_Start_MidNightShift_Successfully_3()
    {
        var createUpdateDailyShift =
           new CreateUpdateDailyShiftDto
           {
               KeeperId = new Guid("d600ff3d-c859-4327-94de-425f16ec9d55"),
               ShiftId = 1,
               OrganizationId = 1,
               GarageId = 1,
               KeeperShiftRequestType = Enums.KeeperShiftRequestType.StartShift,
               TenantId = new Guid(),
               RequestTime = new DateTime(2025, 3, 6, 2, 29, 0)
           };
        var result = await _dailyShiftAppService.CreateAsync(createUpdateDailyShift);
        Assert.NotNull(result);
    }
    [Fact]
    public async Task Should_Connot_Start_MidNightShift_OutOfRangeAfter()
    {
        var createUpdateDailyShift =
           new CreateUpdateDailyShiftDto
           {
               KeeperId = new Guid("d600ff3d-c859-4327-94de-425f16ec9d55"),
               ShiftId = 1,
               OrganizationId = 1,
               GarageId = 1,
               KeeperShiftRequestType = Enums.KeeperShiftRequestType.StartShift,
               TenantId = new Guid(),
               RequestTime = new DateTime(2025, 3, 6, 3, 31, 0)
           };
        await Assert.ThrowsAsync<Exception>(() => _dailyShiftAppService.CreateAsync(createUpdateDailyShift));
    }
    [Fact]
    public async Task Should_Connot_Start_MidNightShift_OutOfRangeBefore()
    {
        var createUpdateDailyShift =
           new CreateUpdateDailyShiftDto
           {
               KeeperId = new Guid("d600ff3d-c859-4327-94de-425f16ec9d55"),
               ShiftId = 1,
               OrganizationId = 1,
               GarageId = 1,
               KeeperShiftRequestType = Enums.KeeperShiftRequestType.StartShift,
               TenantId = new Guid(),
               RequestTime = new DateTime(2025, 3, 6, 22, 29, 0)
           };
        await Assert.ThrowsAsync<Exception>(() => _dailyShiftAppService.CreateAsync(createUpdateDailyShift));
    }

    [Fact]
    public async Task Should_Connot_Start_MidNightShift_OutOfRangeBefore__1()
    {
        var createUpdateDailyShift =
           new CreateUpdateDailyShiftDto
           {
               KeeperId = new Guid("d600ff3d-c859-4327-94de-425f16ec9d55"),
               ShiftId = 1,
               OrganizationId = 1,
               GarageId = 1,
               KeeperShiftRequestType = Enums.KeeperShiftRequestType.StartShift,
               TenantId = new Guid(),
               RequestTime = new DateTime(2025, 3, 6, 20, 29, 0)
           };
        await Assert.ThrowsAsync<Exception>(() => _dailyShiftAppService.CreateAsync(createUpdateDailyShift));
    }
}