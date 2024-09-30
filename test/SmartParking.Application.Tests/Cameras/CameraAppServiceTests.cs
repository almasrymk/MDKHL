using Shouldly;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
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
    [Fact]
    public async Task Should_Create_A_Valid_Camera()
    {
        var result = await _cameraAppService.CreateAsync(
            new CreateUpdateCameraDto
            {
                Name = "Camera1",                  // Valid Name
                Model = "ModelX",                  // Valid Model
                MainType = "TypeA",                // Valid MainType
                BrandName = "BrandZ",              // Valid BrandName
                Code = "C12345",                   // Valid Code
                CameraIP = "192.168.1.100",        // Valid Camera IP
                MacAddress = "00:1A:2B:3C:4D:5E",  // Valid MAC Address
                Type = "Outdoor",                  // Valid Type
                GarageId = 1,                      // Valid Garage ID
                Status = Status.Active,            // Valid Status
            });

        result.ShouldNotBeNull();                  // Ensure camera was created
        result.Name.ShouldBe("Camera1");           // Validate the name of the created camera
    }

    [Fact]
    public async Task Should_Fail_To_Create_Camera_Without_Name()
    {
        var exception = await Should.ThrowAsync<AbpValidationException>(async () =>
        {
            await _cameraAppService.CreateAsync(
                new CreateUpdateCameraDto
                {
                    Name = string.Empty,  // Simulate missing Name
                    Model = "Model1",
                    MainType = "Type1",
                    BrandName = "BrandA",
                    Code = "12345",
                    CameraIP = "192.168.1.1",
                    MacAddress = "00:1B:44:11:3A:B7",
                    Type = "Indoor",
                    GarageId = 1,
                    Status = Status.Active,
                    TenantId = null
                });
        });

        exception.ValidationErrors.ShouldContain(error => error.MemberNames.Contains(nameof(CreateUpdateCameraDto.Name)));
    }
    [Fact]
    public async Task Should_Update_Camera_Type()
    {
        // Step 1: Create a valid camera first
        var createdCamera = await _cameraAppService.CreateAsync(
            new CreateUpdateCameraDto
            {
                Name = "Camera1",                  // Valid Name
                Model = "ModelX",                  // Valid Model
                MainType = "TypeA",                // Valid MainType
                BrandName = "BrandZ",              // Valid BrandName
                Code = "C12345",                   // Valid Code
                CameraIP = "192.168.1.100",        // Valid Camera IP
                MacAddress = "00:1A:2B:3C:4D:5E",  // Valid MAC Address
                Type = "Outdoor",                  // Valid Type
                GarageId = 1,                      // Valid Garage ID
                Status = Status.Active,            // Valid Status
            });

        // Ensure the camera was created successfully
        createdCamera.ShouldNotBeNull();
        createdCamera.Name.ShouldBe("Camera1");
        // Check if the camera ID is valid
        var cameraId = createdCamera.Id;
        cameraId.ShouldBeGreaterThan(0);   // Ensure we have a valid camera ID

        // Step 2: Update the 'Type' of the created camera
        var updatedCameraDto = new CreateUpdateCameraDto
        {
            Name = createdCamera.Name,
            Model = createdCamera.Model,
            MainType = createdCamera.MainType,
            BrandName = createdCamera.BrandName,
            Code = createdCamera.Code,
            CameraIP = createdCamera.CameraIP,
            MacAddress = createdCamera.MacAddress,
            Type = "Indoor",                     // Change Type from 'Outdoor' to 'Indoor'
           // GarageId = createdCamera.GarageId,
            Status = createdCamera.Status,
            TenantId = createdCamera.TenantId
        };

        // Attempt to update the camera
        var updatedCamera = await _cameraAppService.UpdateAsync(cameraId, updatedCameraDto);

        // Step 3: Validate that the camera's type was updated
        updatedCamera.ShouldNotBeNull();
        updatedCamera.Type.ShouldBe("Indoor");    // Verify the updated Type
    }



}
