using AutoMapper.Internal.Mappers;
using SmartParking.Entities.Shifts;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace SmartParking.Shifts;

public abstract class ShiftsAppService_Tests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IShiftAppService _shiftAppService;

    public ShiftsAppService_Tests()
    {
        _shiftAppService = GetRequiredService<IShiftAppService>();
    }
    [Fact]
    public async Task CreateShift_ShouldAllowMultipleShiftsOnSameDay_IfNotOverlapping()
    {
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
            Status=Enums.Status.Active,
            ArabicName = "زعتر",
            EnglishName = "en",
            IsFullDay = false,
            Notes = ""
        };

        var shift2 = new CreateUpdateShiftDto
        {
            Description = "Afternoon Shift",
            StartDate = new DateOnly(2023, 10, 1),
            EndDate = new DateOnly(2023, 10, 12),
            StartTime = new TimeOnly(13, 0), // Does not overlap with shift1
            EndTime = new TimeOnly(17, 0),
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Active,
            ArabicName = "زعتر",
            EnglishName = "en",
            IsFullDay = false,
            Notes =""
        };

        // Act
        await _shiftAppService.CreateAsync(shift1);
        var result = await _shiftAppService.CreateAsync(shift2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
    }

    [Fact]
    public async Task CreateShift_ShouldNotAllowOverlappingShiftsOnSameDay()
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
            Status = Enums.Status.Active,
            ArabicName = "زعتر",
            EnglishName = "en",
            IsFullDay = false,
            Notes = ""

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
            Status = Enums.Status.Active,
            ArabicName = "زعتر",
            EnglishName = "en",
            IsFullDay = false,
            Notes = ""
        };

        // Act
        await _shiftAppService.CreateAsync(shift1);

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
            Status = Enums.Status.Active,
            ArabicName = "زعتر",
            EnglishName = "en",
            IsFullDay = false,
            Notes = ""
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
            Status = Enums.Status.Active,
            ArabicName = "زعتر",
            EnglishName = "en",
            IsFullDay = false,
            Notes = ""
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
            Status = Enums.Status.Active,
            ArabicName = "زعتر",
            EnglishName = "en",
            IsFullDay = false,
            Notes = ""
        };




        // Assert
        var exception = await Assert.ThrowsAsync<Exception>(async() => await _shiftAppService.UpdateAsync(createdShift2.Id, updateShift2));
        Assert.Equal("Shift overlaps with an existing shift.", exception.Message);
    }

    // ❌ Should prevent shifts from overlapping on the same day
    [Fact]
    public async Task CreateShift_ShouldNotAllowOverlappingShifts_SameDay()
    {
        // Arrange
        var shift1 = new CreateUpdateShiftDto
        {
            Description = "Evening Shift",
            StartDate = new DateOnly(2025, 2, 26),
            EndDate = new DateOnly(2025, 2, 26),
            StartTime = new TimeOnly(18, 0),
            EndTime = new TimeOnly(22, 0),
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Active,
            ArabicName = "مسائي",
            EnglishName = "Evening Shift",
            IsFullDay = false,
            Notes = ""
        };

        var shift2 = new CreateUpdateShiftDto
        {
            Description = "Overlap Shift",
            StartDate = new DateOnly(2025, 2, 26),
            EndDate = new DateOnly(2025, 2, 26),
            StartTime = new TimeOnly(21, 0), // Overlaps with shift1 (9 PM - 11 PM)
            EndTime = new TimeOnly(23, 0),
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Active,
            ArabicName = "تداخل",
            EnglishName = "Overlap Shift",
            IsFullDay = false,
            Notes = ""
        };

        await _shiftAppService.CreateAsync(shift1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(async () =>await _shiftAppService.CreateAsync(shift2));
        Assert.Contains("Shift overlaps with an existing shift.", exception.Message);
    }

    [Fact]
    public async Task CreateShift_ShouldAllowInsertingBeforeMidnightShifts()
    {
        // Arrange
        var shift1 = new CreateUpdateShiftDto
        {
            Description = "Night Shift",
            StartDate = new DateOnly(2025, 2, 26),
            EndDate = new DateOnly(2025, 2, 27), // Extends into the next day
            StartTime = new TimeOnly(23, 0), // 11 PM
            EndTime = new TimeOnly(4, 0),   // 4 AM
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Active,
            ArabicName = "الوردية الليلية",
            EnglishName = "Night Shift",
            IsFullDay = false,
            Notes = ""
        };

        var shift2 = new CreateUpdateShiftDto
        {
            Description = "Conflicting Night Shift",
            StartDate = new DateOnly(2025, 2, 26),
            EndDate = new DateOnly(2025, 2, 26),
            StartTime = new TimeOnly(22, 0), 
            EndTime = new TimeOnly(23, 0),
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Active,
            ArabicName = "تداخل ليلي",
            EnglishName = "Conflicting Night Shift",
            IsFullDay = false,
            Notes = ""
        };

        await _shiftAppService.CreateAsync(shift1);
        var result= await _shiftAppService.CreateAsync(shift2);

        // Act & Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
    }

    // ❌ Should prevent creating a shift without StartDate and EndDate
    [Fact]
    public async Task CreateShift_ShouldFail_IfStartDateOrEndDateMissing()
    {
        // Arrange
        var shift = new CreateUpdateShiftDto
        {
            Description = "Invalid Shift",
            StartDate = default, // Missing
            EndDate = new DateOnly(2025, 2, 26),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(12, 0),
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Active,
            ArabicName = "خطأ",
            EnglishName = "Invalid Shift",
            IsFullDay = false,
            Notes = ""
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>await _shiftAppService.CreateAsync(shift));
        Assert.Contains("Value cannot be empty", exception.ValidationErrors[0].ToString());
    }

    // ❌ Should prevent creating a shift with empty StartTime and EndTime when IsFullDay is false
    [Fact]
    public async Task CreateShift_ShouldFail_IfStartTimeAndEndTimeAreEmpty_WhenIsFullDayFalse()
    {
        // Arrange
        var shift = new CreateUpdateShiftDto
        {
            Description = "Invalid Time Shift",
            StartDate = new DateOnly(2025, 2, 26),
            EndDate = new DateOnly(2025, 2, 26),
            StartTime = TimeOnly.MinValue, // 00:00
            EndTime = TimeOnly.MinValue,   // 00:00
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Active,
            ArabicName = "وقت غير صحيح",
            EnglishName = "Invalid Time Shift",
            IsFullDay = false, // Not full day but has empty time
            Notes = ""
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AbpValidationException>(() => _shiftAppService.CreateAsync(shift));
        Assert.Contains("Value cannot be empty: Start Time,End Time", exception.ValidationErrors[0].ToString());
    }
    
    [Fact]
    public async Task CreateTwoShift_ShouldFail_ToOverlapping_MidNightShifts()
    {
        // Arrange
        var shift = new CreateUpdateShiftDto
        {
            Description = "Invalid Time Shift",
            StartDate = new DateOnly(2025, 2, 26),
            EndDate = new DateOnly(2025, 2, 27),
            StartTime = new TimeOnly(22, 0), // Overlaps with shift1
            EndTime = new TimeOnly(4, 0),
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Active,
            ArabicName = "وقت غير صحيح",
            EnglishName = "Invalid Time Shift",
            IsFullDay = false, // Not full day but has empty time
            Notes = ""
        };
        await _shiftAppService.CreateAsync(shift);

        var shift2 = new CreateUpdateShiftDto
        {
            Description = "Invalid Time Shift",
            StartDate = new DateOnly(2025, 2, 26),
            EndDate = new DateOnly(2025, 2, 27),
            StartTime = new TimeOnly(21, 0), // Overlaps with shift1
            EndTime = new TimeOnly(5, 0),
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Active,
            ArabicName = "وقت غير صحيح",
            EnglishName = "Invalid Time Shift",
            IsFullDay = false, // Not full day but has empty time
            Notes = ""
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(async () => await _shiftAppService.CreateAsync(shift2));
        Assert.Contains("Shift overlaps with an existing shift.", exception.Message);
    }


    [Fact]
    public async Task CreateShift_ShouldAllowInsertingThreeShiftsInSequence()
    {
        // Arrange
        var shift1 = new CreateUpdateShiftDto
        {
            Description = "Night Shift",
            StartDate = new DateOnly(2025, 2, 26),
            EndDate = new DateOnly(2025, 2, 27), // Extends into the next day
            StartTime = new TimeOnly(23, 0), // 11 PM
            EndTime = new TimeOnly(7, 0),   // 4 AM
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Inactive,
            ArabicName = "الوردية الليلية",
            EnglishName = "Night Shift",
            IsFullDay = false,
            Notes = ""
        };

        var shift2 = new CreateUpdateShiftDto
        {
            Description = "Day Shift",
            StartDate = new DateOnly(2025, 2, 26),
            EndDate = new DateOnly(2025, 2, 27),
            StartTime = new TimeOnly(7, 1),
            EndTime = new TimeOnly(15, 0),
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Inactive,
            ArabicName = "شيفت صباحي",
            EnglishName = "Day Shift",
            IsFullDay = false,
            Notes = ""
        };
        
        var shift3 = new CreateUpdateShiftDto
        {
            Description = "Last Shift",
            StartDate = new DateOnly(2025, 2, 26),
            EndDate = new DateOnly(2025, 2, 27),
            StartTime = new TimeOnly(15, 1),
            EndTime = new TimeOnly(22, 59),
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Active,
            ArabicName = "شيفت اخير",
            EnglishName = "Last Shift",
            IsFullDay = false,
            Notes = ""
        };

        await _shiftAppService.CreateAsync(shift1);
        var result = await _shiftAppService.CreateAsync(shift2);
        var result2 = await _shiftAppService.CreateAsync(shift3);

        // Act & Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
        Assert.NotNull(result2);
        Assert.Equal(3, result2.Id);
    }
    
    [Fact]
    public async Task UpdateShifts_CheckStatusAndOverlapping()
    {
        // Arrange
        var shift1 = new CreateUpdateShiftDto
        {
            Description = "Night Shift",
            StartDate = new DateOnly(2025, 2, 26),
            EndDate = new DateOnly(2025, 2, 27), // Extends into the next day
            StartTime = new TimeOnly(23, 0), // 11 PM
            EndTime = new TimeOnly(7, 0),   // 4 AM
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Inactive,
            ArabicName = "الوردية الليلية",
            EnglishName = "Night Shift",
            IsFullDay = false,
            Notes = ""
        };

        var shift2 = new CreateUpdateShiftDto
        {
            Description = "Day Shift",
            StartDate = new DateOnly(2025, 2, 26),
            EndDate = new DateOnly(2025, 2, 27),
            StartTime = new TimeOnly(7, 0),
            EndTime = new TimeOnly(15, 0),
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Inactive,
            ArabicName = "شيفت صباحي",
            EnglishName = "Day Shift",
            IsFullDay = false,
            Notes = ""
        };
        
        var shift3 = new CreateUpdateShiftDto
        {
            Description = "Last Shift",
            StartDate = new DateOnly(2025, 2, 26),
            EndDate = new DateOnly(2025, 2, 27),
            StartTime = new TimeOnly(15, 0),
            EndTime = new TimeOnly(23, 0),
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Active,
            ArabicName = "شيفت اخير",
            EnglishName = "Last Shift",
            IsFullDay = false,
            Notes = ""
        };

        var result1=await _shiftAppService.CreateAsync(shift1);
        var result = await _shiftAppService.CreateAsync(shift2);
        var result2 = await _shiftAppService.CreateAsync(shift3);
        shift2.Status=Enums.Status.Active;
        shift2.StartTime = new TimeOnly(7, 1);
        shift2.EndTime = new TimeOnly(14, 59);
        var updatedShift=await _shiftAppService.UpdateAsync(result.Id,shift2);
        shift1.Status=Enums.Status.Active;
        shift1.StartTime = new TimeOnly(23, 1);
        shift1.EndTime = new TimeOnly(6, 59);

        var updatedShiftt=await _shiftAppService.UpdateAsync(result1.Id, shift1);

        Assert.NotNull(updatedShift);
        Assert.NotNull(updatedShiftt);

    }
    [Fact]
    public async Task UpdateShifts_CheckStatusAndOverlappingAndAddFullDayShift()
    {
        // Arrange
        var shift1 = new CreateUpdateShiftDto
        {
            Description = "Night Shift",
            StartDate = new DateOnly(2025, 2, 26),
            EndDate = new DateOnly(2025, 2, 27), // Extends into the next day
            StartTime = new TimeOnly(23, 0), // 11 PM
            EndTime = new TimeOnly(7, 0),   // 4 AM
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Inactive,
            ArabicName = "الوردية الليلية",
            EnglishName = "Night Shift",
            IsFullDay = false,
            Notes = ""
        };

        var shift2 = new CreateUpdateShiftDto
        {
            Description = "Day Shift",
            StartDate = new DateOnly(2025, 2, 26),
            EndDate = new DateOnly(2025, 2, 27),
            StartTime = new TimeOnly(7, 0),
            EndTime = new TimeOnly(15, 0),
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Inactive,
            ArabicName = "شيفت صباحي",
            EnglishName = "Day Shift",
            IsFullDay = false,
            Notes = ""
        };

        var shift3 = new CreateUpdateShiftDto
        {
            Description = "Last Shift",
            StartDate = new DateOnly(2025, 2, 26),
            EndDate = new DateOnly(2025, 2, 27),
            StartTime = new TimeOnly(15, 0),
            EndTime = new TimeOnly(23, 0),
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Active,
            ArabicName = "شيفت اخير",
            EnglishName = "Last Shift",
            IsFullDay = false,
            Notes = ""
        };

        var result1 = await _shiftAppService.CreateAsync(shift1);
        var result = await _shiftAppService.CreateAsync(shift2);
        var result2 = await _shiftAppService.CreateAsync(shift3);
        shift2.Status = Enums.Status.Active;
        shift2.StartTime = new TimeOnly(7, 1);
        shift2.EndTime = new TimeOnly(14, 59);
        var updatedShift = await _shiftAppService.UpdateAsync(result.Id, shift2);
        shift1.Status = Enums.Status.Active;
        shift1.StartTime = new TimeOnly(23, 1);
        shift1.EndTime = new TimeOnly(6, 59);

        var updatedShiftt = await _shiftAppService.UpdateAsync(result1.Id, shift1);

        Assert.NotNull(updatedShift);
        Assert.NotNull(updatedShiftt);
        var shift4 = new CreateUpdateShiftDto
        {
            Description = "Last Shift",
            StartDate = new DateOnly(2025, 2, 26),
            EndDate = new DateOnly(2025, 2, 26),
            StartTime = new TimeOnly(0, 0),
            EndTime = new TimeOnly(0, 0),
            GarageId = 1,
            OrganizationId = 1,
            Status = Enums.Status.Active,
            ArabicName = "شيفت اخير",
            EnglishName = "Last Shift",
            IsFullDay = true,
            Notes = ""
        };
        var exception = await Assert.ThrowsAsync<Exception>(async () => await _shiftAppService.CreateAsync(shift4));
        Assert.Contains("Shift overlaps with an existing shift.", exception.Message);

    }

}
