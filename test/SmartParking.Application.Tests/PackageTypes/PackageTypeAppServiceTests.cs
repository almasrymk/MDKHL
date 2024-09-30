using Microsoft.EntityFrameworkCore;
using Shouldly;
using SmartParking.Lookups;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Xunit;

namespace SmartParking.PackageTypes;

public abstract class PackageTypeAppServiceTests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
   where TStartupModule : IAbpModule
{
    private readonly IPackageTypesAppService _packageTypesAppService;

    protected PackageTypeAppServiceTests()
    {
        _packageTypesAppService = GetRequiredService<IPackageTypesAppService>();
    }

    [Fact]
    public async Task Initial_Data_Should_Contain_PackageType()
    {
        //Act
        var result = await _packageTypesAppService.GetListAsync(new PagedAndSortedResultRequestDto());

        //Assert
        result.TotalCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Should_Create_A_Valid_PackageType()
    {
        //Act
        var result = await _packageTypesAppService.CreateAsync(
            new CreateUpdatePackageTypeDto
            {
                EnglishName = "test",
                ArabicName = "test",
            }
        );

        //Assert
        result.ShouldNotBeNull();
        result.EnglishName.ShouldBe("test");
    }

    [Fact]
    public async Task Should_Not_Create_A_PackageType_Without_ArabicName()
    {

        var exception = await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            await _packageTypesAppService.CreateAsync(
                new CreateUpdatePackageTypeDto
                {
                    EnglishName = "test",
                   
                }
            );
        });

        exception.ShouldNotBeNull();
    }
}
