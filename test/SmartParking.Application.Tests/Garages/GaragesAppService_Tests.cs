using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;


namespace SmartParking.Garages;

public class GaragesAppService_Tests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

    private readonly IGarageAppService _garageAppService;

    public GaragesAppService_Tests()
    {
        _garageAppService = GetRequiredService<IGarageAppService>();

    }

    [Fact]
    public async Task Should_Get_List_Of_Books()
    {
        //Act
        var result = await _garageAppService.GetListAsync(
            new PagedAndSortedResultRequestDto()
        );

        //Assert
        result.TotalCount.ShouldBeGreaterThan(0);
        result.Items.ShouldContain(b => b.Name == "GarageA");
    }
}
