using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Xunit;

namespace SmartParking.Cameras;

public abstract class  CameraAppServiceTests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private ICamerasAppService _cameraAppService;

    protected CameraAppServiceTests()
    {
        _cameraAppService=GetRequiredService<ICamerasAppService>();
    }
    [Fact]
    public async Task Should_Get_List_of_Cameras()
    {
        var result = await _cameraAppService.GetListAsync(
            new PagedAndSortedResultRequestDto()
            );
        result.TotalCount.ShouldBeGreaterThan(0);

    }
}
