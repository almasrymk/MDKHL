using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartParking.Lookups.State;
using SmartParking.Lookups;
using SmartParking.LookupsAppServices;
using Volo.Abp.Modularity;
using SmartParking.Lookups.CameraMainType;
using Xunit;
using Volo.Abp.Domain.Repositories;
using SmartParking.Entities.Lookups;
using Shouldly;
using Volo.Abp.Validation;
using Volo.Abp.Domain.Entities;

namespace SmartParking.CameraMainTypes
{
    public abstract class CameraMainTypeAppServiceTests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
       where TStartupModule : IAbpModule
    {


        private readonly ICameraMainTypeAppService _cameraMainTypeAppService;
        private readonly IRepository<CameraBrand, int> _cameraBrandRepository;

        public CameraMainTypeAppServiceTests()
        {
            _cameraMainTypeAppService = GetRequiredService<ICameraMainTypeAppService>();
            _cameraBrandRepository = GetRequiredService<IRepository<CameraBrand, int>>();
        }

        [Fact]
        public async Task Should_Create_CameraMainType_When_Valid()
        {
            // Arrange
            var brand = await _cameraBrandRepository.InsertAsync(new CameraBrand
            {
                EnglishName = "TestBrand",
                ArabicName = "ماركة اختبار"
            }, autoSave: true);

            var input = new CreateUpdateCameraMainTypeDto
            {
                EnglishName = "MainTypeOne",
                ArabicName = "نوع رئيسي",
                CameraBrandId = brand.Id
            };

            // Act
            var result = await _cameraMainTypeAppService.CreateAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.EnglishName.ShouldBe(input.EnglishName);
            result.ArabicName.ShouldBe(input.ArabicName);
        }


        [Fact]
        public async Task Should_Not_Create_CameraMainType_With_Existing_EnglishName()
        {
            // Arrange
            var brand = await _cameraBrandRepository.InsertAsync(new CameraBrand
            {
                EnglishName = "TestBrand2",
                ArabicName = "ماركة ٢"
            }, autoSave: true);

            await _cameraMainTypeAppService.CreateAsync(new CreateUpdateCameraMainTypeDto
            {
                EnglishName = "DuplicateMainType",
                ArabicName = "نوع مكرر",
                CameraBrandId = brand.Id
            });

            var input = new CreateUpdateCameraMainTypeDto
            {
                EnglishName = "DuplicateMainType", // duplicate
                ArabicName = "نوع جديد",
                CameraBrandId = brand.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _cameraMainTypeAppService.CreateAsync(input));
        }

        [Fact]
        public async Task Should_Not_Create_CameraMainType_With_Existing_ArabicName()
        {
            // Arrange
            var brand = await _cameraBrandRepository.InsertAsync(new CameraBrand
            {
                EnglishName = "TestBrand3",
                ArabicName = "ماركة ٣"
            }, autoSave: true);

            await _cameraMainTypeAppService.CreateAsync(new CreateUpdateCameraMainTypeDto
            {
                EnglishName = "UniqueEnglish",
                ArabicName = "نوع مكرر", // duplicate
                CameraBrandId = brand.Id
            });

            var input = new CreateUpdateCameraMainTypeDto
            {
                EnglishName = "AnotherEnglish",
                ArabicName = "نوع مكرر", // duplicate
                CameraBrandId = brand.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _cameraMainTypeAppService.CreateAsync(input));
        }

        [Fact]
        public async Task Should_Update_CameraMainType_When_Valid()
        {
            // Arrange
            var brand = await _cameraBrandRepository.InsertAsync(new CameraBrand
            {
                EnglishName = "BrandUpdate",
                ArabicName = "ماركة تحديث"
            }, autoSave: true);

            var created = await _cameraMainTypeAppService.CreateAsync(new CreateUpdateCameraMainTypeDto
            {
                EnglishName = "OldName",
                ArabicName = "اسم قديم",
                CameraBrandId = brand.Id
            });

            var input = new CreateUpdateCameraMainTypeDto
            {
                EnglishName = "NewName",
                ArabicName = "اسم جديد",
                CameraBrandId = brand.Id
            };

            // Act
            var updated = await _cameraMainTypeAppService.UpdateAsync(created.Id, input);

            // Assert
            updated.EnglishName.ShouldBe("NewName");
            updated.ArabicName.ShouldBe("اسم جديد");
        }


        [Fact]
        public async Task Should_Not_Update_CameraMainType_With_Existing_EnglishName()
        {
            // Arrange
            var brand = await _cameraBrandRepository.InsertAsync(new CameraBrand
            {
                EnglishName = "BrandUpdate2",
                ArabicName = "ماركة تحديث ٢"
            }, autoSave: true);

            var one = await _cameraMainTypeAppService.CreateAsync(new CreateUpdateCameraMainTypeDto
            {
                EnglishName = "ExistingName",
                ArabicName = "نوع 1",
                CameraBrandId = brand.Id
            });

            var two = await _cameraMainTypeAppService.CreateAsync(new CreateUpdateCameraMainTypeDto
            {
                EnglishName = "AnotherName",
                ArabicName = "نوع 2",
                CameraBrandId = brand.Id
            });

            var input = new CreateUpdateCameraMainTypeDto
            {
                EnglishName = "ExistingName", // duplicate
                ArabicName = "نوع مختلف",
                CameraBrandId = brand.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _cameraMainTypeAppService.UpdateAsync(two.Id, input));
        }
        [Theory]
        [InlineData("", "نوع عربي")]
        [InlineData("Main Type", "")]
        [InlineData("", "")]
        public async Task Should_Not_Create_CameraMainType_With_Missing_Names(string englishName, string arabicName)
        {
            var brand = await _cameraBrandRepository.InsertAsync(new CameraBrand
            {
                EnglishName = "BrandValidation",
                ArabicName = "ماركة صلاحية"
            }, autoSave: true);

            await Assert.ThrowsAsync<AbpValidationException>(() =>
                _cameraMainTypeAppService.CreateAsync(new CreateUpdateCameraMainTypeDto
                {
                    EnglishName = englishName,
                    ArabicName = arabicName,
                    CameraBrandId = brand.Id
                }));
        }
        [Theory]
        [InlineData("", "نوع عربي")]
        [InlineData("Main Type", "")]
        [InlineData("", "")]
        public async Task Should_Not_Update_Camera_Main_Type_With_Missing_Names(string englishName, string arabicName)
        {
            // Arrange
            var brand = await _cameraBrandRepository.InsertAsync(new CameraBrand
            {
                EnglishName = "ValidationBrand",
                ArabicName = "ماركة التحقق"
            }, autoSave: true);

            var created = await _cameraMainTypeAppService.CreateAsync(new CreateUpdateCameraMainTypeDto
            {
                EnglishName = "InitialName",
                ArabicName = "اسم ابتدائي",
                CameraBrandId = brand.Id
            });

            var input = new CreateUpdateCameraMainTypeDto
            {
                EnglishName = englishName,
                ArabicName = arabicName,
                CameraBrandId = brand.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<AbpValidationException>(async () =>
            {
                await _cameraMainTypeAppService.UpdateAsync(created.Id, input);
            });
        }

    }
}
