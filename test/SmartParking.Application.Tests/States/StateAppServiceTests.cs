
using SmartParking.Lookups.Region;
using SmartParking.Lookups;
using System.Threading.Tasks;
using Volo.Abp.Modularity;
using Xunit;
using SmartParking.Lookups.State;
using Volo.Abp.Application.Dtos;
using Shouldly;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SmartParking.Enums;
using SmartParking.LookupsAppServices;
using System;
using Volo.Abp.Validation;

namespace SmartParking.States;

public class StateAppServiceTests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
   where TStartupModule : IAbpModule
{
    private readonly IRegionAppService _regionAppService;
    private readonly IStateAppService _stateAppService;
    private readonly ICountriesAppService _countriesAppService;

    protected StateAppServiceTests()
    {
        _stateAppService = GetRequiredService<IStateAppService>();
        _regionAppService = GetRequiredService<IRegionAppService>();
        _countriesAppService = GetRequiredService<ICountriesAppService>();
    }

    [Fact]
    public async Task Should_Not_Create_State_With_Duplicate_EnglishName()
    {
        // Arrange
        var region = await GetFirstRegionAsync();
        var input1 = new CreateUpdateStateDto
        {
            EnglishName = "Test State",
            ArabicName = "اختبار",
            RegionId = region.Id,
            Status = Status.Active
        };

        await _stateAppService.CreateAsync(input1);

        var input2 = new CreateUpdateStateDto
        {
            EnglishName = "Test State", // duplicate English name
            ArabicName = "تكرار",
            RegionId = region.Id,
            Status = Status.Active
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _stateAppService.CreateAsync(input2));
    }

    [Fact]
    public async Task Should_Not_Create_State_With_Duplicate_ArabicName()
    {
        // Arrange
        var region = await GetFirstRegionAsync();
        var input1 = new CreateUpdateStateDto
        {
            EnglishName = "Unique State",
            ArabicName = "تكرار",
            RegionId = region.Id,
            Status = Status.Active
        };

        await _stateAppService.CreateAsync(input1);

        var input2 = new CreateUpdateStateDto
        {
            EnglishName = "Another Name",
            ArabicName = "تكرار", // duplicate Arabic name
            RegionId = region.Id,
            Status = Status.Active
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _stateAppService.CreateAsync(input2));
    }

    [Fact]
    public async Task Should_Not_Update_State_To_Duplicate_Name()
    {
        // Arrange
        var region = await GetFirstRegionAsync();

        var state1 = await _stateAppService.CreateAsync(new CreateUpdateStateDto
        {
            EnglishName = "State One",
            ArabicName = "حالة واحدة",
            RegionId = region.Id,
            Status = Status.Active
        });

        var state2 = await _stateAppService.CreateAsync(new CreateUpdateStateDto
        {
            EnglishName = "State Two",
            ArabicName = "حالة اثنين",
            RegionId = region.Id,
            Status = Status.Active
        });

        // Attempt to update state2 with state1's English name
        var updateInput = new CreateUpdateStateDto
        {
            EnglishName = "State One", // duplicate
            ArabicName = "تحديث",
            RegionId = region.Id,
            Status = Status.Active
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _stateAppService.UpdateAsync(state2.Id, updateInput));
    }

    [Theory]
    [InlineData(null, "اسم عربي")]
    [InlineData("English Name", null)]
    [InlineData("", "")]
    public async Task Should_Not_Create_State_Without_Required_Fields(string englishName, string arabicName)
    {
        // Arrange
        var region = await GetFirstRegionAsync();

        var input = new CreateUpdateStateDto
        {
            EnglishName = englishName,
            ArabicName = arabicName,
            RegionId = region.Id,
            Status = Status.Active
        };

        // Act & Assert
        await Assert.ThrowsAsync<AbpValidationException>(() =>
            _stateAppService.CreateAsync(input));
    }

    [Fact]
    public async Task Should_Not_Create_State_With_Invalid_Region()
    {
        // Arrange
        var input = new CreateUpdateStateDto
        {
            EnglishName = "Invalid Region State",
            ArabicName = "منطقة غير موجودة",
            RegionId = 99999, // Assuming this region ID doesn't exist
            Status = Status.Active
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _stateAppService.CreateAsync(input));
    }

    // Helper method to get a valid region
    private async Task<RegionLookupDto> GetFirstRegionAsync()
    {
        var countries = await _countriesAppService.GetCountryLookupAsync();
        var egypt = countries.FirstOrDefault(c => c.EnglishName == "Egypt");
        var regions = await _regionAppService.GetRegionByCountry(egypt.Id);
        return regions.First();
    }
}
