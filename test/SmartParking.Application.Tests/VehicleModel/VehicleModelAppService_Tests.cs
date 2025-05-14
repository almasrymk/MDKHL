using Abp.Application.Services.Dto;
using Shouldly;
using SmartParking.Lookups.VehicleModel;
using SmartParking.Vehicles.LookUps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Xunit;
namespace SmartParking.VehicleModel;
/// <summary>
/// Unit tests for the VehicleModelsAppService.
/// </summary>
public class VehicleModelAppService_Tests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IVehicleModelsAppService _vehicleModelAppService;

    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleModelAppService_Tests{TStartupModule}"/> class.
    /// </summary>
    public VehicleModelAppService_Tests()
    {
        _vehicleModelAppService = GetRequiredService<IVehicleModelsAppService>();
    }

    /// <summary>
    /// Tests that a valid vehicle model can be created.
    /// </summary>
    [Fact]
    public async Task Should_Create_Valid_VehicleModel()
    {
        var input = new CreateUpdateVehicleModelDto
        {
            EnglishName = "Sedan",
            ArabicName = "سيدان",
            VehiclesBrandId = 1
        };

        var result = await _vehicleModelAppService.CreateAsync(input);

        Assert.NotNull(result);
        result.EnglishName.ShouldBe("Sedan");
        result.VehiclesBrandId.ShouldBe(1);
    }

    /// <summary>
    /// Tests that creating a duplicate vehicle model throws an exception.
    /// </summary>
    [Fact]
    public async Task Should_Throw_When_Creating_Duplicate_VehicleModel()
    {
        var input = new CreateUpdateVehicleModelDto
        {
            EnglishName = "Hatchback",
            ArabicName = "هاتشباك",
            VehiclesBrandId = 1
        };

        await _vehicleModelAppService.CreateAsync(input);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _vehicleModelAppService.CreateAsync(input);
        });
    }

    /// <summary>
    /// Tests that a vehicle model can be updated successfully.
    /// </summary>
    [Fact]
    public async Task Should_Update_VehicleModel_Successfully()
    {
        var create = await _vehicleModelAppService.CreateAsync(new CreateUpdateVehicleModelDto
        {
            EnglishName = "SUV",
            ArabicName = "دفع رباعي",
            VehiclesBrandId = 1
        });

        var update = new CreateUpdateVehicleModelDto
        {
            EnglishName = "Crossover",
            ArabicName = "كروس أوفر",
            VehiclesBrandId = 1
        };

        var result = await _vehicleModelAppService.UpdateAsync(create.Id, update);

        result.EnglishName.ShouldBe("Crossover");
    }

    /// <summary>
    /// Tests that updating a vehicle model to a name that already exists throws an exception.
    /// </summary>
    [Fact]
    public async Task Should_Throw_When_Updating_To_Existing_Model_Name()
    {
        var first = await _vehicleModelAppService.CreateAsync(new CreateUpdateVehicleModelDto
        {
            EnglishName = "Convertible",
            ArabicName = "قابل للتحويل",
            VehiclesBrandId = 1
        });

        var second = await _vehicleModelAppService.CreateAsync(new CreateUpdateVehicleModelDto
        {
            EnglishName = "Truck",
            ArabicName = "شاحنة",
            VehiclesBrandId = 1
        });

        var update = new CreateUpdateVehicleModelDto
        {
            EnglishName = "Convertible",
            ArabicName = "قابل للتحويل",
            VehiclesBrandId = 1
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _vehicleModelAppService.UpdateAsync(second.Id, update));
    }

    /// <summary>
    /// Tests that a vehicle model can be deleted successfully.
    /// </summary>
    [Fact]
    public async Task Should_Delete_VehicleModel_Successfully()
    {
        var created = await _vehicleModelAppService.CreateAsync(new CreateUpdateVehicleModelDto
        {
            EnglishName = "Van",
            ArabicName = "فان",
            VehiclesBrandId = 1
        });

        await _vehicleModelAppService.DeleteAsync(created.Id);

        var result = await _vehicleModelAppService.GetAllVehicleModelsAsync();
        result.ShouldNotContain(x => x.Id == created.Id);
    }

    /// <summary>
    /// Tests that deleting a non-existing vehicle model throws EntityNotFoundException.
    /// </summary>
    [Fact]
    public async Task Should_Throw_When_Deleting_Non_Existing_Model()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            _vehicleModelAppService.DeleteAsync(9999));
    }

    /// <summary>
    /// Tests that all vehicle models can be retrieved.
    /// </summary>
    [Fact]
    public async Task Should_Get_All_VehicleModels()
    {
        var result = await _vehicleModelAppService.GetAllVehicleModelsAsync();
        Assert.NotNull(result);
        result.ShouldBeAssignableTo<List<VehicleModelDto>>();
    }

    /// <summary>
    /// Tests filtering vehicle models by brand ID.
    /// </summary>
    [Fact]
    public async Task Should_Filter_By_Brand()
    {
        var created = await _vehicleModelAppService.CreateAsync(new CreateUpdateVehicleModelDto
        {
            EnglishName = "Electric",
            ArabicName = "كهربائي",
            VehiclesBrandId = 2
        });

        var result = await _vehicleModelAppService.GetModelsByBrandAsync(2);

        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// Tests getting models for an unknown brand returns a failed result.
    /// </summary>
    [Fact]
    public async Task Should_Return_NotFound_For_Unknown_Brand()
    {
        var result = await _vehicleModelAppService.GetModelsByBrandAsync(9999);
        Assert.False(result.IsSuccess);
    }

    /// <summary>
    /// Tests searching vehicle models by a keyword returns matching results.
    /// </summary>
    [Fact]
    public async Task Should_Search_VehicleModels_By_Keyword()
    {
        await _vehicleModelAppService.CreateAsync(new CreateUpdateVehicleModelDto
        {
            EnglishName = "Hybrid",
            ArabicName = "هجين",
            VehiclesBrandId = 1
        });

        var result = await _vehicleModelAppService.Search(new PagedAndSortedAndSearchResultRequestDto
        {
            SearchingTerm = "Hybrid",
            MaxResultCount = 10,
            SkipCount = 0
        });

        result.Items.ShouldContain(x => x.EnglishName == "Hybrid");
    }

    /// <summary>
    /// Tests that vehicle models can be retrieved with pagination.
    /// </summary>
    [Fact]
    public async Task Should_Get_List_With_Pagination()
    {
        var result = await _vehicleModelAppService.GetListAsync(new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = 20,
            SkipCount = 0
        });

        Assert.NotNull(result);
        result.Items.Count.ShouldBeLessThanOrEqualTo(20);
    }
}
