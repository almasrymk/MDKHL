using AutoMapper.Internal.Mappers;
using SmartParking.Entities.Shifts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Modularity;
using Xunit;

namespace SmartParking.Shifts;

public class ShiftsAppService_Testss<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IShiftAppService _shiftAppService;

    public ShiftsAppService_Testss()
    {
        _shiftAppService = GetRequiredService<IShiftAppService>();
    }
    [Fact]
    public void CreateShift_ShouldAllowMultipleShiftsOnSameDay_IfNotOverlapping()
    {
        // Arrange
        var shift1 = new CreateUpdateShiftDto
        {
            Description = "Morning Shift",
            StartDate = new DateOnly(2023, 10, 1),
            EndDate = new DateOnly(2023, 10, 1),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(12, 0),
            GarageId = 1,
            OrganizationId = 1,
            Status=Enums.Status.Active
        };

        var shift2 = new CreateUpdateShiftDto
        {
            Description = "Afternoon Shift",
            StartDate = new DateOnly(2023, 10, 1),
            EndDate = new DateOnly(2023, 10, 1),
            StartTime = new TimeOnly(13, 0), // Does not overlap with shift1
            EndTime = new TimeOnly(17, 0),
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Active
        };

        // Act
        _shiftAppService.CreateAsync(shift1);
        var result = _shiftAppService.CreateAsync(shift2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
    }

    [Fact]
    public async void CreateShift_ShouldNotAllowOverlappingShiftsOnSameDay()
    {
        // Arrange
        var shift1 = new CreateUpdateShiftDto
        {
            Description = "Morning Shift",
            StartDate = new DateOnly(2023, 10, 1),
            EndDate = new DateOnly(2023, 10, 1),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(12, 0),
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Active
        };

        var shift2 = new CreateUpdateShiftDto
        {
            Description = "Overlapping Shift",
            StartDate = new DateOnly(2023, 10, 1),
            EndDate = new DateOnly(2023, 10, 1),
            StartTime = new TimeOnly(10, 0), // Overlaps with shift1
            EndTime = new TimeOnly(14, 0),
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Active
        };

        // Act
        _shiftAppService.CreateAsync(shift1);

        // Assert
        var exception =await Assert.ThrowsAsync<Exception>(async () => await _shiftAppService.CreateAsync(shift2));
        Assert.Equal("Shift overlaps with an existing shift.", exception.Message);
    }

    [Fact]
    public async Task UpdateShift_ShouldNotAllowOverlappingShiftsOnSameDay()
    {
        // Arrange
        var shift1 = new CreateUpdateShiftDto
        {
            Description = "Morning Shift",
            StartDate = new DateOnly(2023, 10, 1),
            EndDate = new DateOnly(2023, 10, 1),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(12, 0),
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Active
        };

        var shift2 = new CreateUpdateShiftDto
        {
            Description = "Afternoon Shift",
            StartDate = new DateOnly(2023, 10, 1),
            EndDate = new DateOnly(2023, 10, 1),
            StartTime = new TimeOnly(13, 0),
            EndTime = new TimeOnly(17, 0),
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Active
        };

        await _shiftAppService.CreateAsync(shift1);
        var createdShift2 = await _shiftAppService.CreateAsync(shift2);

        // Act
        createdShift2.StartTime = new TimeOnly(10, 0);
        createdShift2.EndTime = new TimeOnly(14, 0);
        var updateShift2 = new CreateUpdateShiftDto
        {
            Description = createdShift2.Description,
            StartDate = createdShift2.StartDate,
            EndDate = createdShift2.EndDate,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(14, 0),
            GarageId = createdShift2.GarageId,
            OrganizationId = createdShift2.OrganizationId,
            Status = Enums.Status.Active
        };




        // Assert
        var exception = await Assert.ThrowsAsync<Exception>(async() => await _shiftAppService.UpdateAsync(createdShift2.Id, updateShift2));
        Assert.Equal("Shift overlaps with an existing shift.", exception.Message);
    }
}
