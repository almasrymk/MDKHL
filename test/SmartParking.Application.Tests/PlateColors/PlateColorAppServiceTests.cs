using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartParking.Lookups.Region;
using SmartParking.Lookups;
using Volo.Abp.Modularity;
using SmartParking.Lookups.PlateColor;
using Xunit;
using SmartParking.Vehicles.LookUps;
using Volo.Abp.Validation;
using Shouldly;

namespace SmartParking.PlateColors
{
    public abstract class PlateColorAppServiceTests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
       where TStartupModule : IAbpModule
    {
        private readonly IPlateColorsAppService _plateColorAppService;

        public PlateColorAppServiceTests()
        {
            _plateColorAppService = GetRequiredService<IPlateColorsAppService>();
        }

        [Theory]
        [InlineData("", "أحمر")]
        [InlineData("Red", "")]
        [InlineData("", "")]
        public async Task Should_Not_Create_PlateColor_With_Empty_Names(string englishName, string arabicName)
        {
            var input = new CreateUpdatePlateColorDto
            {
                EnglishName = englishName,
                ArabicName = arabicName
            };

            await Assert.ThrowsAsync<AbpValidationException>(async () =>
            {
                await _plateColorAppService.CreateAsync(input);
            });
        }
        [Fact]
        public async Task Should_Not_Create_PlateColor_With_Duplicate_Names()
        {
            var input = new CreateUpdatePlateColorDto
            {
                EnglishName = "Brownish Yellow",
                ArabicName = "بني مصفر"
            };

            // Insert the initial one
            await _plateColorAppService.CreateAsync(input);

            // Attempt to insert duplicate
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _plateColorAppService.CreateAsync(input);
            });
        }
        [Theory]
        [InlineData("", "ازرق")]
        [InlineData("blue", "")]
        [InlineData("", "")]
        public async Task Should_Not_Update_PlateColor_With_Empty_Names(string englishName, string arabicName)
        {
            var created = await _plateColorAppService.CreateAsync(new CreateUpdatePlateColorDto
            {
                EnglishName = "Original",
                ArabicName = "الأصل"
            });

            var input = new CreateUpdatePlateColorDto
            {
                EnglishName = englishName,
                ArabicName = arabicName
            };

            await Assert.ThrowsAsync<AbpValidationException>(async () =>
            {
                await _plateColorAppService.UpdateAsync(created.Id, input);
            });
        }
        [Fact]
        public async Task Should_Not_Update_PlateColor_With_Duplicate_Names()
        {
            // Insert two distinct plate colors
            var red = await _plateColorAppService.CreateAsync(new CreateUpdatePlateColorDto
            {
                EnglishName = "Foshia",
                ArabicName = "بمبي"
            });

            var blue = await _plateColorAppService.CreateAsync(new CreateUpdatePlateColorDto
            {
                EnglishName = "Mint",
                ArabicName = "نعناع"
            });

            var duplicateInput = new CreateUpdatePlateColorDto
            {
                EnglishName = "Foshia", // Same as red
                ArabicName = "بمبي"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _plateColorAppService.UpdateAsync(blue.Id, duplicateInput);
            });

        }
        [Fact]
        public async Task Should_Create_PlateColor_Successfully()
        {
            var input = new CreateUpdatePlateColorDto
            {
                EnglishName = "Orange",
                ArabicName = "برتقالي"
            };

            var result = await _plateColorAppService.CreateAsync(input);

            result.ShouldNotBeNull();
            result.EnglishName.ShouldBe("Orange");
            result.ArabicName.ShouldBe("برتقالي");
        }
        [Fact]
        public async Task Should_Update_PlateColor_Successfully()
        {
            var created = await _plateColorAppService.CreateAsync(new CreateUpdatePlateColorDto
            {
                EnglishName = "Pink",
                ArabicName = "وردي"
            });

            var input = new CreateUpdatePlateColorDto
            {
                EnglishName = "Magenta",
                ArabicName = "أرجواني"
            };

            var updated = await _plateColorAppService.UpdateAsync(created.Id, input);

            updated.EnglishName.ShouldBe("Magenta");
            updated.ArabicName.ShouldBe("أرجواني");
        }
        [Fact]
        public async Task Should_Delete_PlateColor_Successfully()
        {
            var created = await _plateColorAppService.CreateAsync(new CreateUpdatePlateColorDto
            {
                EnglishName = "Teal",
                ArabicName = "تركواز"
            });

            await _plateColorAppService.DeleteAsync(created.Id);

            var all = await _plateColorAppService.GetAllPlateColorsAsync();
            all.Any(x => x.Id == created.Id).ShouldBeFalse();
        }






    }
}
