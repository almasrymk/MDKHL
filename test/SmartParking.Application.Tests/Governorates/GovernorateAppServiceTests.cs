using Microsoft.EntityFrameworkCore;
using Shouldly;
using SmartParking.Entities;
using SmartParking.Lookups;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Xunit;

namespace SmartParking.Governorates;

public abstract class GovernorateAppServiceTests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
   where TStartupModule : IAbpModule
{
    private readonly ICountriesAppService _countriesAppService;
    private readonly IGovernorateAppService _governorateAppService;

    protected GovernorateAppServiceTests()
    {
        _countriesAppService = GetRequiredService<ICountriesAppService>();
        _governorateAppService = GetRequiredService<IGovernorateAppService>();
    }

    [Fact]
    public async Task Initial_Data_Should_Contain_Governorate()
    {
        //Act
        var result = await _governorateAppService.GetListAsync(new PagedAndSortedResultRequestDto());

        //Assert
        result.TotalCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Should_Create_A_Valid_Governorate()
    {
        var countries = await _countriesAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        var country = countries.Items.First();
        //Act
        var result = await _governorateAppService.CreateAsync(
            new CreateUpdateGovernorateDto
            {
                EnglishName = "Alexandria",
                ArabicName = "الإسكندرية",
                ArabicCapital = "الإسكندرية",
                EnglishCapital = "Alexandria",
                CountryId = country.Id,
            }
        );

        //Assert
        result.ShouldNotBeNull();
        result.EnglishName.ShouldBe("Alexandria");
    }

    [Fact]
    public async Task Should_Not_Create_A_Governorate_Without_ArabicName()
    {
        var countries = await _countriesAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        var country = countries.Items.First();

        var exception = await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            await _governorateAppService.CreateAsync(
                new CreateUpdateGovernorateDto
                {
                    EnglishName = "Alexandria",
                    ArabicCapital = "الإسكندرية",
                    EnglishCapital = "Alexandria",
                    CountryId = country.Id,
                }
            );
        });

        exception.ShouldNotBeNull();
    }
}
