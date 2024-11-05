using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SmartParking.Enums;
using SmartParking.Organizations;
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
    private readonly IOrganizationsAppService _organizationAppService;

    public GaragesAppService_Tests()
    {
        _garageAppService = GetRequiredService<IGarageAppService>();
        _organizationAppService = GetRequiredService<IOrganizationsAppService>();
    }

    private async Task<int> GetOrganizationId()
    {
        var organizations = await _organizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        var organization = organizations.Items.FirstOrDefault();
        return organization.Id;
    }


    [Fact]
    public async Task Should_Get_Single_Garage_By_Id()
    {

        var oraganizationId=await GetOrganizationId();
        var garage = await _garageAppService.CreateAsync(new CreateUpdateGarageDto
        {
            NameEn = "Test Garage",
            NameAr = "تيست موقف",
            MaxCapacity = 100,
            ClosingHours = new TimeOnly(18, 0),
            OrganizationId = oraganizationId,
            Status = Status.Active
        });

        var result = await _garageAppService.GetAsync(garage.Id);

        result.ShouldNotBeNull();
        result.NameEn.ShouldBe("Test Garage");
    }

    
    [Fact]
    public async Task Should_Get_List_Of_Garages()
    {
        var oraganizationId = await GetOrganizationId();

        await _garageAppService.CreateAsync(new CreateUpdateGarageDto
        {
            NameEn = "Garage 1",
            NameAr = "موقف 1",
            MaxCapacity = 50,
            ClosingHours = new TimeOnly(17, 0),
            OrganizationId = oraganizationId,
            Status = Status.Active
        });

        await _garageAppService.CreateAsync(new CreateUpdateGarageDto
        {
            NameEn = "Garage 2",
            NameAr = "موقف 2",
            MaxCapacity = 150,
            ClosingHours = new TimeOnly(20, 0),
            OrganizationId = oraganizationId,
            Status = Status.Inactive
        });

        var result = await _garageAppService.GetListAsync(new PagedAndSortedResultRequestDto());

        result.Items.Count.ShouldBeGreaterThan(1);
    }

    [Fact]
    public async Task Should_Create_New_Garage()
    {
        var oraganizationId = await GetOrganizationId();

        var input = new CreateUpdateGarageDto
        {
            NameEn = "New Garage",
            NameAr = "موقف جديد",
            MaxCapacity = 200,
            ClosingHours = new TimeOnly(21, 0),
            OrganizationId = oraganizationId,
            Status = Status.Active
        };

        var result = await _garageAppService.CreateAsync(input);

        result.Id.ShouldBeGreaterThan(0); 
        result.NameEn.ShouldBe("New Garage");
        result.MaxCapacity.ShouldBe(200);
    }

    [Fact]
    public async Task Should_Not_Create_Garage_With_Invalid_Name()
    {
        var oraganizationId = await GetOrganizationId();

        var input = new CreateUpdateGarageDto
        {
            NameEn = "123Invalid",  // Invalid: doesn't start with a letter
            NameAr = "غير صحيح",  // Invalid: doesn't start with a letter
            MaxCapacity = 200,
            ClosingHours = new TimeOnly(21, 0),
            OrganizationId = oraganizationId,
            Status = Status.Active
        };

         var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
            await _garageAppService.CreateAsync(input));

        exception.ValidationErrors.ShouldContain(err => err.MemberNames.Contains(nameof(CreateUpdateGarageDto.NameEn)));
    }

    

    [Fact]
    public async Task Should_Not_Create_Garage_With_Invalid_MaxCapacity()
    {
        var oraganizationId = await GetOrganizationId();

        var input = new CreateUpdateGarageDto
        {
            NameEn = "Valid Garage",
            NameAr = "موقف صحيح",
            MaxCapacity = 1000,  // Invalid: exceeds the upper limit
            ClosingHours = new TimeOnly(21, 0),
            OrganizationId = oraganizationId,
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
            NameEn = "Valid Garage",
            NameAr = "موقف صحيح",
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
