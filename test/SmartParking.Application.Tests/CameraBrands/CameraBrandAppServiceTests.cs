using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using SmartParking.Entities.Lookups;
using SmartParking.Lookups;
using SmartParking.Lookups.CameraBrands;
using SmartParking.LookupsAppServices;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace SmartParking.CameraBrands
{
    public abstract class CameraBrandAppServiceTests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
        where TStartupModule : IAbpModule
    {
        private readonly ICameraBrandAppService _cameraBrandAppService;
        private readonly IRepository<CameraBrand, int> _cameraBrandRepository;

        public CameraBrandAppServiceTests()
        {
            _cameraBrandAppService = GetRequiredService<ICameraBrandAppService>();
            _cameraBrandRepository = GetRequiredService<IRepository<CameraBrand, int>>();
        }

        [Fact]
        public async Task Should_Create_CameraBrand_When_Valid()
        {
            // Arrange
            var input = new CreateUpdateCameraBrandDto
            {
                EnglishName = "UniqueBrand",
                ArabicName = "ماركة فريدة"
            };

            // Act
            var result = await _cameraBrandAppService.CreateAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.EnglishName.ShouldBe(input.EnglishName);
            result.ArabicName.ShouldBe(input.ArabicName);
        }

        [Fact]
        public async Task Should_Not_Create_CameraBrand_With_Existing_EnglishName()
        {
            // Arrange
            await _cameraBrandAppService.CreateAsync(new CreateUpdateCameraBrandDto
            {
                EnglishName = "DuplicateBrand",
                ArabicName = "ماركة مختلفة"
            });

            var input = new CreateUpdateCameraBrandDto
            {
                EnglishName = "DuplicateBrand", // duplicate
                ArabicName = "ماركة جديدة"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _cameraBrandAppService.CreateAsync(input));
        }

        [Fact]
        public async Task Should_Not_Create_CameraBrand_With_Existing_ArabicName()
        {
            // Arrange
            await _cameraBrandAppService.CreateAsync(new CreateUpdateCameraBrandDto
            {
                EnglishName = "AnotherBrand",
                ArabicName = "ماركة مكررة"
            });

            var input = new CreateUpdateCameraBrandDto
            {
                EnglishName = "BrandNew",
                ArabicName = "ماركة مكررة" // duplicate
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _cameraBrandAppService.CreateAsync(input));
        }

        [Fact]
        public async Task Should_Update_CameraBrand_When_Valid()
        {
            // Arrange
            var created = await _cameraBrandAppService.CreateAsync(new CreateUpdateCameraBrandDto
            {
                EnglishName = "OldBrand",
                ArabicName = "ماركة قديمة"
            });

            var input = new CreateUpdateCameraBrandDto
            {
                EnglishName = "UpdatedBrand",
                ArabicName = "ماركة محدثة"
            };

            // Act
            var updated = await _cameraBrandAppService.UpdateAsync(created.Id, input);

            // Assert
            updated.EnglishName.ShouldBe("UpdatedBrand");
            updated.ArabicName.ShouldBe("ماركة محدثة");
        }

        [Fact]
        public async Task Should_Not_Update_CameraBrand_With_Existing_EnglishName()
        {
            // Arrange
            var one = await _cameraBrandAppService.CreateAsync(new CreateUpdateCameraBrandDto
            {
                EnglishName = "ExistBrand",
                ArabicName = "ماركة ١"
            });

            var two = await _cameraBrandAppService.CreateAsync(new CreateUpdateCameraBrandDto
            {
                EnglishName = "AnotherBrand",
                ArabicName = "ماركة ٢"
            });

            var input = new CreateUpdateCameraBrandDto
            {
                EnglishName = "ExistBrand", // duplicate
                ArabicName = "ماركة جديدة"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _cameraBrandAppService.UpdateAsync(two.Id, input));
        }

        [Fact]
        public async Task Should_Not_Update_CameraBrand_With_Existing_ArabicName()
        {
            // Arrange
            var one = await _cameraBrandAppService.CreateAsync(new CreateUpdateCameraBrandDto
            {
                EnglishName = "BrandOne",
                ArabicName = "ماركة مكررة"
            });

            var two = await _cameraBrandAppService.CreateAsync(new CreateUpdateCameraBrandDto
            {
                EnglishName = "BrandTwo",
                ArabicName = "ماركة مختلفة"
            });

            var input = new CreateUpdateCameraBrandDto
            {
                EnglishName = "BrandUpdated",
                ArabicName = "ماركة مكررة" // duplicate
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _cameraBrandAppService.UpdateAsync(two.Id, input));
        }
        [Theory]
        [InlineData("", "اسم عربي")]
        [InlineData("English Name", "")]
        [InlineData("", "")]
        public async Task Should_Not_Create_Camera_Brand_With_Missing_Names(string englishName, string arabicName)
        {
            // Act & Assert
            await Assert.ThrowsAsync<AbpValidationException>(async () =>
            {
                await _cameraBrandAppService.CreateAsync(new CreateUpdateCameraBrandDto
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
        public async Task Should_Not_Update_Camera_Brand_With_Missing_Names(string englishName, string arabicName)
        {
            // Arrange
            var created = await _cameraBrandAppService.CreateAsync(new CreateUpdateCameraBrandDto
            {
                EnglishName = "TempBrand",
                ArabicName = "ماركة مؤقتة"
            });

            var input = new CreateUpdateCameraBrandDto
            {
                EnglishName = englishName,
                ArabicName = arabicName
            };

            // Act & Assert
            await Assert.ThrowsAsync<AbpValidationException>(async () =>
            {
                await _cameraBrandAppService.UpdateAsync(created.Id, input);
            });
        }
        [Fact]
        public async Task Should_Delete_Camera_Brand()
        {
            // Arrange
            var created = await _cameraBrandAppService.CreateAsync(new CreateUpdateCameraBrandDto
            {
                EnglishName = "BrandToDelete",
                ArabicName = "ماركة للحذف"
            });

            // Act
            await _cameraBrandAppService.DeleteAsync(created.Id);

            // Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
            {
                await _cameraBrandAppService.UpdateAsync(created.Id, new CreateUpdateCameraBrandDto
                {
                    EnglishName = "Updated",
                    ArabicName = "محدث"
                });
            });
        }


    }
}
