
using Abp.Runtime.Validation;
using Shouldly;
using SmartParking.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Xunit;

namespace SmartParking.Countries;

public abstract class CountryAppServiceTests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
   where TStartupModule : IAbpModule
{
    private readonly ICountriesAppService _countriesApp;

    protected CountryAppServiceTests()
    {
        _countriesApp = GetRequiredService<ICountriesAppService>();
    }

    [Fact]
    public async Task Initial_Data_Should_Contain_Country()
    {
        //Act
        var result = await _countriesApp.GetListAsync(new PagedAndSortedResultRequestDto());

        //Assert
        result.TotalCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Should_Create_A_Valid_Country()
    {
        //Act
        var result = await _countriesApp.CreateAsync(
            new CreateUpdateCountryDto
            {
                EnglishName = "Afghanistan",
                ArabicName = "افغانستان",
                EnglishCapital = "Kabul",
                ArabicCapital = "كابول"
            }
        );

        //Assert
        result.ShouldNotBeNull();
        result.EnglishName.ShouldBe("Afghanistan");
    }

    [Fact]
    public async Task Should_Not_Create_A_Season_Without_ArabicName()
    {
        var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
        {
            await _countriesApp.CreateAsync(
                new CreateUpdateCountryDto
                {
                    EnglishName = "Afghanistan",
                   
                    EnglishCapital = "Kabul",
                    ArabicCapital = "كابول"
                }
            );
        });

        exception.ValidationErrors
            .ShouldContain(err => err.MemberNames.Any(mem => mem == "ArabicName"));
    }
}
