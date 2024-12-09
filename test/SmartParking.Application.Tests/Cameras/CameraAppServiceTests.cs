using Shouldly;
using SmartParking.Enums;
using SmartParking.Garages;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace SmartParking.Cameras;

public abstract class CameraAppServiceTests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private ICamerasAppService _cameraAppService;
    private IGarageAppService _garagesAppService;

    protected CameraAppServiceTests()
    {
        _cameraAppService = GetRequiredService<ICamerasAppService>();
        _garagesAppService = GetRequiredService<IGarageAppService>();
    }
    [Fact]
    public async Task Should_Get_List_of_Cameras()
    {
        var result = await _cameraAppService.GetListAsync(
            new PagedAndSortedResultRequestDto()
            );
        result.TotalCount.ShouldBeGreaterThan(0);

    }
    [Fact]
    public async Task Should_Create_A_Valid_Camera()
    {
        var garages = await _garagesAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        var garage = garages.Items.First();


        var result = await _cameraAppService.CreateAsync(
            new CreateUpdateCameraDto
            {
                Name = "Camera1",                  // Valid Name
                                                   //  Model = CameraModel.PanasonicLumix,                  // Valid Model
                                                   //MainType = MainCameraType.Action,                // Valid MainType
                                                   // BrandName = CameraBrand.Panasonic,              // Valid BrandName
                DeviceId = "C12345",                   // Valid Code
                CameraIP = "192.168.1.100",        // Valid Camera IP
                MacAddress = "00:1A:2B:3C:4D:5E",  // Valid MAC Address
                //Type = "Outdoor",                  // Valid Type
                CameraType = CameraType.Exit,
                GarageId = garage.Id,                      // Valid Garage ID
                Status = Status.Active,            // Valid Status
            });

        result.ShouldNotBeNull();                  // Ensure camera was created
        result.Name.ShouldBe("Camera1");           // Validate the name of the created camera
    }

    [Fact]
    public async Task Should_Fail_To_Create_Camera_Without_Name()
    {
        var garages = await _garagesAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        var garage = garages.Items.First();
        var exception = await Should.ThrowAsync<AbpValidationException>(async () =>
        {
            await _cameraAppService.CreateAsync(
                new CreateUpdateCameraDto
                {
                    Name = string.Empty,  // Simulate missing Name
                                          // Model = CameraModel.PanasonicLumix,
                                          // MainType = MainCameraType.Action,
                                          //  BrandName = CameraBrand.Panasonic,
                    DeviceId = "12345",
                    CameraIP = "192.168.1.1",
                    MacAddress = "00:1B:44:11:3A:B7",
                    //Type = "Indoor",
                    CameraType = CameraType.Entry,
                    GarageId = garage.Id,
                    Status = Status.Active,
                    TenantId = null
                });
        });

        exception.ValidationErrors.ShouldContain(error => error.MemberNames.Contains(nameof(CreateUpdateCameraDto.Name)));
    }



}
