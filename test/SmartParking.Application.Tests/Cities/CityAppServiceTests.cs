using Microsoft.EntityFrameworkCore;
using Shouldly;
using SmartParking.Entities;
using SmartParking.Lookups;
using SmartParking.Lookups.State;
using SmartParking.LookupsAppServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Xunit;

namespace SmartParking.Cities;

public abstract class CityAppServiceTests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
   where TStartupModule : IAbpModule
{
    private readonly ICitiesAppService _citiesAppService;
    private readonly IStateAppService _stateAppService;

    protected CityAppServiceTests()
    {
        _citiesAppService = GetRequiredService<ICitiesAppService>();
        _stateAppService = GetRequiredService<StateAppService>();
    }

    [Fact]
    public async Task Should_Create_City_When_Valid()
    {
        // Arrange
        var input = new CreateUpdateCityDto
        {
            EnglishName = "NewCity",
            ArabicName = "مدينة جديدة",
            StateId = 1 
        };

        // Act
        var result = await _citiesAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.EnglishName.ShouldBe(input.EnglishName);
    }

    [Fact]
    public async Task Should_Not_Create_City_With_Existing_EnglishName()
    {
        // Arrange
        var existingCity = await _citiesAppService.CreateAsync(new CreateUpdateCityDto
        {
            EnglishName = "DuplicateCity",
            ArabicName = "مدينة مكررة",
            StateId = 1
        });

        var input = new CreateUpdateCityDto
        {
            EnglishName = "DuplicateCity",
            ArabicName = "مدينة جديدة",
            StateId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _citiesAppService.CreateAsync(input));
    }

    [Fact]
    public async Task Should_Not_Create_City_With_Existing_ArabicName()
    {
        // Arrange
        var existingCity = await _citiesAppService.CreateAsync(new CreateUpdateCityDto
        {
            EnglishName = "UniqueCity",
            ArabicName = "مدينة مكررة",
            StateId = 1
        });

        var input = new CreateUpdateCityDto
        {
            EnglishName = "AnotherCity",
            ArabicName = "مدينة مكررة",
            StateId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _citiesAppService.CreateAsync(input));
    }

    [Fact]
    public async Task Should_Update_City_When_Valid()
    {
        // Arrange
        var city = await _citiesAppService.CreateAsync(new CreateUpdateCityDto
        {
            EnglishName = "UpdateCity",
            ArabicName = "مدينة للتحديث",
            StateId = 1
        });

        var input = new CreateUpdateCityDto
        {
            EnglishName = "UpdatedCity",
            ArabicName = "مدينة محدثة",
            StateId = 1
        };

        // Act
        var updated = await _citiesAppService.UpdateAsync(city.Id, input);

        // Assert
        updated.EnglishName.ShouldBe("UpdatedCity");
    }

    [Fact]
    public async Task Should_Not_Update_City_With_Existing_EnglishName()
    {
        // Arrange
        var city1 = await _citiesAppService.CreateAsync(new CreateUpdateCityDto
        {
            EnglishName = "CityOne",
            ArabicName = "مدينة 1",
            StateId = 1
        });

        var city2 = await _citiesAppService.CreateAsync(new CreateUpdateCityDto
        {
            EnglishName = "CityTwo",
            ArabicName = "مدينة 2",
            StateId = 1
        });

        var input = new CreateUpdateCityDto
        {
            EnglishName = "CityOne",
            ArabicName = "مدينة أخرى",
            StateId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _citiesAppService.UpdateAsync(city2.Id, input));
    }


}
