
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

namespace SmartParking.States;

public class StateAppServiceTests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
   where TStartupModule : IAbpModule
{
    private readonly IRegionAppService _regionAppService;
    private readonly IStateAppService _stateAppService;

    protected StateAppServiceTests()
    {
        _stateAppService = GetRequiredService<IStateAppService>();
        _regionAppService = GetRequiredService<IRegionAppService>();
    }

    [Fact]
    public async Task Initial_Data_Should_Contain_State()
    {
        //Act
        var result = await _stateAppService.GetListAsync(new PagedAndSortedResultRequestDto());

        //Assert
        result.TotalCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Should_Create_A_Valid_State()
    {
        var regions = await _regionAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        var region = regions.Items.First();
        //Act
        var result = await _stateAppService.CreateAsync(
            new CreateUpdateStateDto
            {
                EnglishName = "test",
                ArabicName = "test",
                Status = Enums.Status.Inactive,
                RegionId = region.Id,
            }
        );

        //Assert
        result.ShouldNotBeNull();
        result.EnglishName.ShouldBe("test");
    }

    [Fact]
    public async Task Should_Not_Create_A_State_Without_ArabicName()
    {
        var countries = await _regionAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        var country = countries.Items.First();

        var exception = await Assert.ThrowsAsync<DbUpdateException>((System.Func<Task>)(async () =>
        {
            await this._regionAppService.CreateAsync(
                new CreateUpdateRegionDto
                {
                    EnglishName = "test",
                    Status = Enums.Status.Inactive,
                    CountryId = country.Id,
                }
            );
        }));

        exception.ShouldNotBeNull();
    }
}
