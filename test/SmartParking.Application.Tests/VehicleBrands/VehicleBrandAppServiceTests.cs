using System;
using System.Threading.Tasks;
using Shouldly;
using SmartParking.Entities.Lookups;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Lookups.VehicleBrand;
using SmartParking.LookupsAppServices;
using SmartParking.Vehicles.LookUps;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace SmartParking.VehicleBrands
{
    public abstract class VehicleBrandAppServiceTests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
        where TStartupModule : IAbpModule
    {
        private readonly VehicleBrandsAppService _vehicleBrandAppService;

        public VehicleBrandAppServiceTests()
        {
            _vehicleBrandAppService = GetRequiredService<VehicleBrandsAppService>();
        }

        [Fact]
        public async Task Should_Create_VehicleBrand_When_Valid()
        {
            // Arrange
            var input = new CreateUpdateVehicleBrandDto
            {
                EnglishName = "UniqueBrand",
                ArabicName = "ماركة فريدة"
            };

            // Act
            var result = await _vehicleBrandAppService.CreateAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.EnglishName.ShouldBe(input.EnglishName);
            result.ArabicName.ShouldBe(input.ArabicName);
        }

        [Fact]
        public async Task Should_Not_Create_VehicleBrand_With_Existing_EnglishName()
        {
            // Arrange
            await _vehicleBrandAppService.CreateAsync(new CreateUpdateVehicleBrandDto
            {
                EnglishName = "DuplicateBrand",
                ArabicName = "ماركة مختلفة"
            });

            var input = new CreateUpdateVehicleBrandDto
            {
                EnglishName = "DuplicateBrand", // duplicate
                ArabicName = "ماركة جديدة"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _vehicleBrandAppService.CreateAsync(input));
        }

        [Fact]
        public async Task Should_Not_Create_VehicleBrand_With_Existing_ArabicName()
        {
            // Arrange
            await _vehicleBrandAppService.CreateAsync(new CreateUpdateVehicleBrandDto
            {
                EnglishName = "AnotherBrand",
                ArabicName = "ماركة مكررة"
            });

            var input = new CreateUpdateVehicleBrandDto
            {
                EnglishName = "BrandNew",
                ArabicName = "ماركة مكررة" // duplicate
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _vehicleBrandAppService.CreateAsync(input));
        }

        [Fact]
        public async Task Should_Update_VehicleBrand_When_Valid()
        {
            // Arrange
            var created = await _vehicleBrandAppService.CreateAsync(new CreateUpdateVehicleBrandDto
            {
                EnglishName = "OldBrand",
                ArabicName = "ماركة قديمة"
            });

            var input = new CreateUpdateVehicleBrandDto
            {
                EnglishName = "UpdatedBrand",
                ArabicName = "ماركة محدثة"
            };

            // Act
            var updated = await _vehicleBrandAppService.UpdateAsync(created.Id, input);

            // Assert
            updated.EnglishName.ShouldBe("UpdatedBrand");
            updated.ArabicName.ShouldBe("ماركة محدثة");
        }

        [Fact]
        public async Task Should_Not_Update_VehicleBrand_With_Existing_EnglishName()
        {
            // Arrange
            var one = await _vehicleBrandAppService.CreateAsync(new CreateUpdateVehicleBrandDto
            {
                EnglishName = "ExistBrand",
                ArabicName = "ماركة ١"
            });

            var two = await _vehicleBrandAppService.CreateAsync(new CreateUpdateVehicleBrandDto
            {
                EnglishName = "AnotherBrand",
                ArabicName = "ماركة ٢"
            });

            var input = new CreateUpdateVehicleBrandDto
            {
                EnglishName = "ExistBrand", // duplicate
                ArabicName = "ماركة جديدة"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _vehicleBrandAppService.UpdateAsync(two.Id, input));
        }

        [Fact]
        public async Task Should_Not_Update_VehicleBrand_With_Existing_ArabicName()
        {
            // Arrange
            var one = await _vehicleBrandAppService.CreateAsync(new CreateUpdateVehicleBrandDto
            {
                EnglishName = "BrandOne",
                ArabicName = "ماركة مكررة"
            });

            var two = await _vehicleBrandAppService.CreateAsync(new CreateUpdateVehicleBrandDto
            {
                EnglishName = "BrandTwo",
                ArabicName = "ماركة مختلفة"
            });

            var input = new CreateUpdateVehicleBrandDto
            {
                EnglishName = "BrandUpdated",
                ArabicName = "ماركة مكررة" // duplicate
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _vehicleBrandAppService.UpdateAsync(two.Id, input));
        }

        [Theory]
        [InlineData("", "اسم عربي")]
        [InlineData("English Name", "")]
        [InlineData("", "")]
        public async Task Should_Not_Create_Vehicle_Brand_With_Missing_Names(string englishName, string arabicName)
        {
            // Act & Assert
            await Assert.ThrowsAsync<AbpValidationException>(async () =>
            {
                await _vehicleBrandAppService.CreateAsync(new CreateUpdateVehicleBrandDto
                {
                    EnglishName = englishName,
                    ArabicName = arabicName
                });
            });
        }

        [Theory]
        [InlineData("", "اسم عربي")]
        [InlineData("English Name", "")]
        [InlineData("", "")]
        public async Task Should_Not_Update_Vehicle_Brand_With_Missing_Names(string englishName, string arabicName)
        {
            // Arrange
            var created = await _vehicleBrandAppService.CreateAsync(new CreateUpdateVehicleBrandDto
            {
                EnglishName = "TempBrand",
                ArabicName = "ماركة مؤقتة"
            });

            var input = new CreateUpdateVehicleBrandDto
            {
                EnglishName = englishName,
                ArabicName = arabicName
            };

            // Act & Assert
            await Assert.ThrowsAsync<AbpValidationException>(async () =>
            {
                await _vehicleBrandAppService.UpdateAsync(created.Id, input);
            });
        }

        [Fact]
        public async Task Should_Delete_Vehicle_Brand()
        {
            // Arrange
            var created = await _vehicleBrandAppService.CreateAsync(new CreateUpdateVehicleBrandDto
            {
                EnglishName = "BrandToDelete",
                ArabicName = "ماركة للحذف"
            });

            // Act
            await _vehicleBrandAppService.DeleteAsync(created.Id);

            // Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
            {
                await _vehicleBrandAppService.UpdateAsync(created.Id, new CreateUpdateVehicleBrandDto
                {
                    EnglishName = "Updated",
                    ArabicName = "محدث"
                });
            });
        }
    }
}
