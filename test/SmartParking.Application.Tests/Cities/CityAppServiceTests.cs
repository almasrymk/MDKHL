using Microsoft.EntityFrameworkCore;
using Shouldly;
using SmartParking.Entities;
using SmartParking.Lookups;
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
    private readonly IGovernorateAppService _governorateAppService;

    protected CityAppServiceTests()
    {
        _citiesAppService = GetRequiredService<ICitiesAppService>();
        _governorateAppService = GetRequiredService<IGovernorateAppService>();
    }

    [Fact]
    public async Task Initial_Data_Should_Contain_City()
    {
        //Act
        var result = await _citiesAppService.GetListAsync(new PagedAndSortedResultRequestDto());

        //Assert
        result.TotalCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Should_Create_A_Valid_City()
    {
        var governorates = await _governorateAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        var governorate = governorates.Items.First();
        //Act
        var result = await _citiesAppService.CreateAsync(
            new CreateUpdateCityDto
            {
                EnglishName = "Alexandria",
                ArabicName = "الإسكندرية",
                GovernorateId = governorate.Id,
            }
        );

        //Assert
        result.ShouldNotBeNull();
        result.EnglishName.ShouldBe("Alexandria");
    }

    [Fact]
    public async Task Should_Not_Create_A_City_Without_ArabicName()
    {
        var governorates = await _governorateAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        var governorate = governorates.Items.First();

        var exception = await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            await _citiesAppService.CreateAsync(
                new CreateUpdateCityDto
                {
                    EnglishName = "Alexandria",
                    GovernorateId = governorate.Id,
                }
            );
        });

        exception.ShouldNotBeNull();
    }
}
