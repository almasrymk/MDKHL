using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using SmartParking.Lookups;
using SmartParking.Lookups.LicenceType;
using SmartParking.Lookups.PlateColor;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace SmartParking.LicenseTypes
{
    public abstract class LicenseTypeAppServiceTests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
      where TStartupModule : IAbpModule
    {
        private readonly IPlateColorsAppService _plateColorAppService;
        private readonly ILicenceTypesAppService _licenseTypeAppService;

        public LicenseTypeAppServiceTests()
        {
            _plateColorAppService = GetRequiredService<IPlateColorsAppService>();
            _licenseTypeAppService = GetRequiredService<ILicenceTypesAppService>();
        }


        [Fact]
        public async Task Should_Create_LicenseType_When_Valid()
        {
            // Arrange
            var plateColor = await _plateColorAppService.CreateAsync(new CreateUpdatePlateColorDto
            {
                EnglishName = "Red",
                ArabicName = "أحمر"
            });

            var input = new CreateUpdateLicenceTypeDto
            {
                EnglishName = "Standard",
                ArabicName = "قياسي",
                PlateColorId = plateColor.Id 
            };

            // Act
            var result = await _licenseTypeAppService.CreateAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.EnglishName.ShouldBe(input.EnglishName);
            result.ArabicName.ShouldBe(input.ArabicName);
            result.PlateColorId.ShouldBe(plateColor.Id);
        }
        [Fact]
        public async Task Should_Not_Create_LicenseType_When_PlateColor_Missing()
        {
            // Arrange
            var input = new CreateUpdateLicenceTypeDto
            {
                EnglishName = "Standard",
                ArabicName = "قياسي",
                PlateColorId = 0 // Invalid PlateColorId
            };

            // Act & Assert
            await Assert.ThrowsAsync<AbpValidationException>(() => _licenseTypeAppService.CreateAsync(input));
        }
        [Fact]
        public async Task Should_Not_Create_LicenseType_With_Existing_EnglishName()
        {
            // Arrange
            var plateColor = await _plateColorAppService.CreateAsync(new CreateUpdatePlateColorDto
            {
                EnglishName = "Red",
                ArabicName = "أحمر"
            });

            // Create first LicenseType
            await _licenseTypeAppService.CreateAsync(new CreateUpdateLicenceTypeDto
            {
                EnglishName = "Standard",
                ArabicName = "قياسي",
                PlateColorId = plateColor.Id
            });

            // Attempt to create another LicenseType with duplicate EnglishName
            var input = new CreateUpdateLicenceTypeDto
            {
                EnglishName = "Standard", // duplicate
                ArabicName = "مكرر",
                PlateColorId = plateColor.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _licenseTypeAppService.CreateAsync(input));
        }

        [Fact]
        public async Task Should_Update_LicenseType_When_Valid()
        {
            // Arrange
            var plateColor = await _plateColorAppService.CreateAsync(new CreateUpdatePlateColorDto
            {
                EnglishName = "Blue",
                ArabicName = "أزرق"
            });

            var created = await _licenseTypeAppService.CreateAsync(new CreateUpdateLicenceTypeDto
            {
                EnglishName = "OldType",
                ArabicName = "نوع قديم",
                PlateColorId = plateColor.Id
            });

            var input = new CreateUpdateLicenceTypeDto
            {
                EnglishName = "NewType",
                ArabicName = "نوع جديد",
                PlateColorId = plateColor.Id
            };

            // Act
            var updated = await _licenseTypeAppService.UpdateAsync(created.Id, input);

            // Assert
            updated.EnglishName.ShouldBe("NewType");
            updated.ArabicName.ShouldBe("نوع جديد");
        }
        [Fact]
        public async Task Should_Not_Update_LicenseType_With_Existing_EnglishName()
        {
            // Arrange
            var plateColor = await _plateColorAppService.CreateAsync(new CreateUpdatePlateColorDto
            {
                EnglishName = "Green",
                ArabicName = "أخضر"
            });

            // Create first LicenseType
            var first = await _licenseTypeAppService.CreateAsync(new CreateUpdateLicenceTypeDto
            {
                EnglishName = "Standard",
                ArabicName = "قياسي",
                PlateColorId = plateColor.Id
            });

            // Create second LicenseType
            var second = await _licenseTypeAppService.CreateAsync(new CreateUpdateLicenceTypeDto
            {
                EnglishName = "Premium",
                ArabicName = "متميز",
                PlateColorId = plateColor.Id
            });

            // Attempt to update the second LicenseType with a duplicate EnglishName
            var input = new CreateUpdateLicenceTypeDto
            {
                EnglishName = "Standard", // duplicate
                ArabicName = "مختلف",
                PlateColorId = plateColor.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _licenseTypeAppService.UpdateAsync(second.Id, input));
        }
        [Theory]
        [InlineData("", "نوع عربي")]
        [InlineData("Main Type", "")]
        [InlineData("", "")]
        public async Task Should_Not_Update_LicenseType_With_Missing_Names(string englishName, string arabicName)
        {
            // Arrange
            var plateColor = await _plateColorAppService.CreateAsync(new CreateUpdatePlateColorDto
            {
                EnglishName = "Black",
                ArabicName = "أسود"
            });

            var created = await _licenseTypeAppService.CreateAsync(new CreateUpdateLicenceTypeDto
            {
                EnglishName = "Basic",
                ArabicName = "أساسي",
                PlateColorId = plateColor.Id
            });

            var input = new CreateUpdateLicenceTypeDto
            {
                EnglishName = englishName,
                ArabicName = arabicName,
                PlateColorId = plateColor.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<AbpValidationException>(() =>
                _licenseTypeAppService.UpdateAsync(created.Id, input));
        }





    }
}
