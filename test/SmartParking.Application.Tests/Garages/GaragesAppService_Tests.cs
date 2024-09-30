using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SmartParking.Enums;
using SmartParking.OrganizationsGarages;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;


namespace SmartParking.Garages;

public abstract class GaragesAppService_Tests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

    private readonly IGarageAppService _garageAppService;

    public GaragesAppService_Tests()
    {
        _garageAppService = GetRequiredService<IGarageAppService>();

    }

    [Fact]
    public async Task Should_Get_Single_Garage_By_Id()
    {
        var garageId = 1;
        var garage = await _garageAppService.CreateAsync(new CreateUpdateGarageDto
        {
            Name = "Test Garage",
            MaxCapacity = 100,
            ClosingHours = new TimeOnly(18, 0),
            OrganizationId = 3,
            Status = Status.Active
        });

        var result = await _garageAppService.GetAsync(garageId);

        result.ShouldNotBeNull();
        result.Name.ShouldBe("Test Garage");
    }

    [Fact]
    public async Task Should_Get_List_Of_Garages()
    {
        await _garageAppService.CreateAsync(new CreateUpdateGarageDto
        {
            Name = "Garage 1",
            MaxCapacity = 50,
            ClosingHours = new TimeOnly(17, 0),
            OrganizationId = 3,
            Status = Status.Active
        });

        await _garageAppService.CreateAsync(new CreateUpdateGarageDto
        {
            Name = "Garage 2",
            MaxCapacity = 150,
            ClosingHours = new TimeOnly(20, 0),
            OrganizationId = 3,
            Status = Status.Inactive
        });

        var result = await _garageAppService.GetListAsync(new PagedAndSortedResultRequestDto());

        result.Items.Count.ShouldBeGreaterThan(1);
    }

    [Fact]
    public async Task Should_Create_New_Garage()
    {
        var input = new CreateUpdateGarageDto
        {
            Name = "New Garage",
            MaxCapacity = 200,
            ClosingHours = new TimeOnly(21, 0),
            OrganizationId = 3,
            Status = Status.Active
        };

        var result = await _garageAppService.CreateAsync(input);

        result.Id.ShouldBeGreaterThan(0); 
        result.Name.ShouldBe("New Garage");
        result.MaxCapacity.ShouldBe(200);
    }

    [Fact]
    public async Task Should_Not_Create_Garage_With_Invalid_Name()
    {
        var input = new CreateUpdateGarageDto
        {
            Name = "123Invalid",  // Invalid: doesn't start with a letter
            MaxCapacity = 200,
            ClosingHours = new TimeOnly(21, 0),
            OrganizationId = 5,
            Status = Status.Active
        };

         var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
            await _garageAppService.CreateAsync(input));

        exception.ValidationErrors.ShouldContain(err => err.MemberNames.Contains(nameof(CreateUpdateGarageDto.Name)));
    }

    

    [Fact]
    public async Task Should_Not_Create_Garage_With_Invalid_MaxCapacity()
    {
        var input = new CreateUpdateGarageDto
        {
            Name = "Valid Garage",
            MaxCapacity = 1000,  // Invalid: exceeds the upper limit
            ClosingHours = new TimeOnly(21, 0),
            OrganizationId = 5,
            Status = Status.Active
        };

       var exception= await Assert.ThrowsAsync<AbpValidationException>(async () =>
            await _garageAppService.CreateAsync(input));
        exception.ValidationErrors.ShouldContain(err => err.MemberNames.Contains(nameof(CreateUpdateGarageDto.MaxCapacity)));
    }

    [Fact]
    public async Task Should_Not_Create_Garage_With_Invalid_OrganizationId()
    {
        var input = new CreateUpdateGarageDto
        {
            Name = "Valid Garage",
            MaxCapacity = 200,
            ClosingHours = new TimeOnly(21, 0),
            OrganizationId = 0,  // Invalid: should be greater than 0
            Status = Status.Active
        };

        var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
            await _garageAppService.CreateAsync(input));
        exception.ValidationErrors.ShouldContain(err=>err.MemberNames.Contains(nameof(CreateUpdateGarageDto.OrganizationId)));
    }

}
