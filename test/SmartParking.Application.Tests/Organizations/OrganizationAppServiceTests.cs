
using Microsoft.EntityFrameworkCore;
using Shouldly;
using SmartParking.Lookups;
using SmartParking.Lookups.Region;
using SmartParking.Lookups.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace SmartParking.Organizations;

public abstract class OrganizationAppServiceTests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
   where TStartupModule : IAbpModule
{
    private readonly IOrganizationsAppService _organizationsRepository;
    private readonly ICitiesAppService _CityRepository;
    private readonly ICountriesAppService _CountryRepository;
    private readonly IStateAppService _StateRepository;
    private readonly IRegionAppService _RegionRepository;

    protected OrganizationAppServiceTests()
    {
        _organizationsRepository = base.GetRequiredService<IOrganizationsAppService>();
        _CityRepository = base.GetRequiredService<ICitiesAppService>();
        _CountryRepository = base.GetRequiredService<ICountriesAppService>();
        _StateRepository = base.GetRequiredService<IStateAppService>();
        _RegionRepository = base.GetRequiredService<IRegionAppService>();
    }

    [Fact]
    public async Task Initial_Data_Should_Contain_Organization()
    {
        //Act
        var result = await _organizationsRepository.GetListAsync(new PagedAndSortedResultRequestDto());

        //Assert
        result.TotalCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Should_Create_A_Valid_Organization()
    {
        var countries = await _CountryRepository.GetListAsync(new PagedAndSortedResultRequestDto());
        var country = countries.Items.FirstOrDefault();
        var cities = await _CityRepository.GetListAsync(new PagedAndSortedResultRequestDto());
        var city = cities.Items.FirstOrDefault();
        var states = await _StateRepository.GetListAsync(new PagedAndSortedResultRequestDto());
        var state = states.Items.FirstOrDefault();
        var regions = await _RegionRepository.GetListAsync(new PagedAndSortedResultRequestDto());
        var region = regions.Items.FirstOrDefault();
        var organization = new CreateUpdateOrganiztionDto
        {
            ArabicName = "اسلام",
            EnglishName = "islam",
            CityId = city.Id,
            CountryId = country.Id,
            StateId = state.Id,
            RegionId = region.Id,
            ContactMail = "islam@gmail.com",
            CRExpiryDate = new DateOnly(2025, 12, 31),
            LicenseExpiryDate = new DateOnly(2026, 12, 31),
            MaxCapacity = 120,
            NumOfCameras = 5,
            PhoneNum = "012345664",
            Status = Enums.Status.Active,
            UnifiedLicenseNum = "920126451",
            WhatsAppNum = "+966531294567"

        };
        //Act
        var result = await _organizationsRepository.CreateAsync(organization);

        //Assert
        result.ShouldNotBeNull();
        result.EnglishName.ShouldBe("islam");
    }

    [Fact]
    public async Task Should_Not_Create_A_Organization_Without_EnglishName()
    {
        var countries = await _CountryRepository.GetListAsync(new PagedAndSortedResultRequestDto());
        var country = countries.Items.FirstOrDefault();
        var cities = await _CityRepository.GetListAsync(new PagedAndSortedResultRequestDto());
        var city = cities.Items.FirstOrDefault();
        var states = await _StateRepository.GetListAsync(new PagedAndSortedResultRequestDto());
        var state = states.Items.FirstOrDefault();
        var regions = await _RegionRepository.GetListAsync(new PagedAndSortedResultRequestDto());
        var region = regions.Items.FirstOrDefault();
        var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
        {
            await _organizationsRepository.CreateAsync(
                new CreateUpdateOrganiztionDto
                {
                    ArabicName = "الشركة الثانية",
                    
                    CityId = city.Id,
                    CountryId = country.Id,
                    StateId = state.Id,
                    RegionId = region.Id,
                    ContactMail = "info@secondorganization.com",
                    CRExpiryDate = new DateOnly(2024, 5, 15),
                    LicenseExpiryDate = new DateOnly(2025, 5, 15),
                    MaxCapacity = 120,
                    NumOfCameras = 5,
                    PhoneNum = "012345674",
                    Status = Enums.Status.Active,
                    UnifiedLicenseNum = "920123454",
                    WhatsAppNum = "+966531234565"
                }
            );
        });

        exception.ShouldNotBeNull();
    }
}
