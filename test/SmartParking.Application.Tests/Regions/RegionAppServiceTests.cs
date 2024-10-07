using Microsoft.EntityFrameworkCore;
using Shouldly;
using SmartParking.Lookups;
using SmartParking.Lookups.Region;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Xunit;

namespace SmartParking.Regions;

public class RegionAppServiceTests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
   where TStartupModule : IAbpModule
{
    private readonly ICountriesAppService _countriesAppService;
    private readonly IRegionAppService _regionAppService;

    protected RegionAppServiceTests()
    {
        _countriesAppService = GetRequiredService<ICountriesAppService>();
        _regionAppService = GetRequiredService<IRegionAppService>();
    }

    [Fact]
    public async Task Initial_Data_Should_Contain_Region()
    {
        //Act
        var result = await _regionAppService.GetListAsync(new PagedAndSortedResultRequestDto());

        //Assert
        result.TotalCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Should_Create_A_Valid_Region()
    {
        var countries = await _countriesAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        var country = countries.Items.First();
        //Act
        var result = await _regionAppService.CreateAsync(
            new CreateUpdateRegionDto
            {
                EnglishName = "test",
                ArabicName = "test",
                Status = Enums.Status.Active,
                CountryId = country.Id,
            }
        );

        //Assert
        result.ShouldNotBeNull();
        result.EnglishName.ShouldBe("test");
    }

    [Fact]
    public async Task Should_Not_Create_A_Region_Without_ArabicName()
    {
        var countries = await _countriesAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        var country = countries.Items.First();

        var exception = await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            await _regionAppService.CreateAsync(
                new CreateUpdateRegionDto
                {
                    EnglishName = "test",
                    Status = Enums.Status.Active,
                    CountryId = country.Id,
                }
            );
        });

        exception.ShouldNotBeNull();
    }
}
