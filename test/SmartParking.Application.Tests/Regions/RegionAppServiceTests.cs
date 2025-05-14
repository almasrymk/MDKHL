using Microsoft.EntityFrameworkCore;
using Shouldly;
using SmartParking.Entities.Lookups;
using SmartParking.Enums;
using SmartParking.Lookups;
using SmartParking.Lookups.Region;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace SmartParking.Regions;

public class RegionAppServiceTests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
   where TStartupModule : IAbpModule
{
    private readonly IRegionAppService _regionAppService;
    private readonly ICountriesAppService _countriesAppService;

    public RegionAppServiceTests()
    {
        _regionAppService = GetRequiredService<IRegionAppService>();
        _countriesAppService = GetRequiredService<ICountriesAppService>();
    }

    [Fact]
    public async Task Should_Get_Regions_By_Country()
    {
        // Arrange
        var countries = await _countriesAppService.GetCountryLookupAsync();
        var egypt = countries.FirstOrDefault(c => c.EnglishName == "Egypt");

        // Act
        var regions = await _regionAppService.GetRegionByCountry(egypt.Id);

        // Assert
        regions.ShouldNotBeNull();
        regions.Count.ShouldBeGreaterThan(0);
        egypt.ShouldNotBeNull();
    }

    [Fact]
    public async Task Should_Create_New_Region()
    {
        // Arrange
        var countries = await _countriesAppService.GetCountryLookupAsync();
        var egypt = countries.FirstOrDefault(c => c.EnglishName == "Egypt");

        var input = new CreateUpdateRegionDto
        {
            EnglishName = "New Region",
            ArabicName = "منطقة جديدة",
            CountryId = egypt.Id,
            Status = Status.Active
        };

        // Act
        var result = await _regionAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.EnglishName.ShouldBe("New Region");
        result.ArabicName.ShouldBe("منطقة جديدة");
        result.Status.ShouldBe(Status.Active);
        result.CountryId.ShouldBe(egypt.Id);
    }

    [Fact]
    public async Task Should_Not_Allow_Duplicate_Region_Names()
    {
        // Arrange
        var countries = await _countriesAppService.GetCountryLookupAsync();
        var egypt = countries.FirstOrDefault(c => c.EnglishName == "Egypt");

        var input = new CreateUpdateRegionDto
        {
            EnglishName = "test", // Already exists in seed
            ArabicName = "اختبار", // Different Arabic name
            CountryId = egypt.Id,
            Status = Status.Active
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _regionAppService.CreateAsync(input);
        });
    }

    [Fact]
    public async Task Should_Update_Existing_Region()
    {
        // Arrange
        var countries = await _countriesAppService.GetCountryLookupAsync();
        var egypt = countries.FirstOrDefault(c => c.EnglishName == "Egypt");

        var regions = await _regionAppService.GetRegionByCountry(egypt.Id);
        var regionToUpdate = regions.First();

        var input = new CreateUpdateRegionDto
        {
            EnglishName = "Updated Region",
            ArabicName = "تحديث",
            CountryId = egypt.Id,
            Status = Status.Inactive
        };

        // Act
        var result = await _regionAppService.UpdateAsync(regionToUpdate.Id, input);

        // Assert
        result.ShouldNotBeNull();
        result.EnglishName.ShouldBe("Updated Region");
        result.Status.ShouldBe(Status.Inactive);
    }

    [Fact]
    public async Task Should_Delete_Region()
    {
        // Arrange
        var countries = await _countriesAppService.GetCountryLookupAsync();
        var egypt = countries.FirstOrDefault(c => c.EnglishName == "Egypt");

        var input = new CreateUpdateRegionDto
        {
            EnglishName = "Region to Delete",
            ArabicName = "منطقة للحذف",
            CountryId = egypt.Id,
            Status = Status.Active
        };

        var created = await _regionAppService.CreateAsync(input);

        // Act
        await _regionAppService.DeleteAsync(created.Id);

        // Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
        {
            await _regionAppService.UpdateAsync(created.Id, input);
        });
    }
  
    
    [Theory]
    [InlineData("", "اسم عربي")]
    [InlineData("English Name", "")]
    [InlineData("", "")]
    public async Task Should_Not_Update_Region_With_Empty_Names(string englishName, string arabicName)
    {
        // Arrange
        var countries = await _countriesAppService.GetCountryLookupAsync();
        var egypt = countries.FirstOrDefault(c => c.EnglishName == "Egypt");

        var regions = await _regionAppService.GetRegionByCountry(egypt.Id);
        var regionToUpdate = regions.First();

        var input = new CreateUpdateRegionDto
        {
            EnglishName = englishName,
            ArabicName = arabicName,
            CountryId = egypt.Id,
            Status = Status.Active
        };

        // Act & Assert
        await Assert.ThrowsAsync<AbpValidationException>(async () =>
        {
            await _regionAppService.UpdateAsync(regionToUpdate.Id, input);
        });
    }


}
