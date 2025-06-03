using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using SmartParking.Entities.Lookups;
using SmartParking.Lookups.CameraModel;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace SmartParking.CameraModels
{
    public abstract class CameraModelAppServiceTests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
         where TStartupModule : IAbpModule
    {
        private readonly ICameraModelAppService _cameraModelAppService;
        private readonly IRepository<CameraBrand, int> _cameraBrandRepository;

        public CameraModelAppServiceTests()
        {
            _cameraModelAppService = GetRequiredService<ICameraModelAppService>();
            _cameraBrandRepository = GetRequiredService<IRepository<CameraBrand, int>>();
        }

        [Fact]
        public async Task Should_Create_CameraModel_When_Valid()
        {
            // Arrange
            var brand = await _cameraBrandRepository.InsertAsync(new CameraBrand
            {
                EnglishName = "ModelBrand1",
                ArabicName = "ماركة موديل ١"
            }, autoSave: true);

            var input = new CreateUpdateCameraModelDto
            {
                EnglishName = "ModelOne",
                ArabicName = "موديل ١",
                CameraBrandId = brand.Id
            };

            // Act
            var result = await _cameraModelAppService.CreateAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.EnglishName.ShouldBe(input.EnglishName);
            result.ArabicName.ShouldBe(input.ArabicName);
        }

        [Fact]
        public async Task Should_Not_Create_CameraModel_With_Existing_EnglishName()
        {
            // Arrange
            var brand = await _cameraBrandRepository.InsertAsync(new CameraBrand
            {
                EnglishName = "ModelBrand2",
                ArabicName = "ماركة موديل ٢"
            }, autoSave: true);

            await _cameraModelAppService.CreateAsync(new CreateUpdateCameraModelDto
            {
                EnglishName = "DuplicateModel",
                ArabicName = "موديل مكرر",
                CameraBrandId = brand.Id
            });

            var input = new CreateUpdateCameraModelDto
            {
                EnglishName = "DuplicateModel", // duplicate
                ArabicName = "موديل جديد",
                CameraBrandId = brand.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _cameraModelAppService.CreateAsync(input));
        }

        [Fact]
        public async Task Should_Not_Create_CameraModel_With_Existing_ArabicName()
        {
            // Arrange
            var brand = await _cameraBrandRepository.InsertAsync(new CameraBrand
            {
                EnglishName = "ModelBrand3",
                ArabicName = "ماركة موديل ٣"
            }, autoSave: true);

            await _cameraModelAppService.CreateAsync(new CreateUpdateCameraModelDto
            {
                EnglishName = "UniqueModel",
                ArabicName = "موديل مكرر",
                CameraBrandId = brand.Id
            });

            var input = new CreateUpdateCameraModelDto
            {
                EnglishName = "NewModel",
                ArabicName = "موديل مكرر", // duplicate
                CameraBrandId = brand.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _cameraModelAppService.CreateAsync(input));
        }

        [Fact]
        public async Task Should_Update_CameraModel_When_Valid()
        {
            // Arrange
            var brand = await _cameraBrandRepository.InsertAsync(new CameraBrand
            {
                EnglishName = "ModelBrand4",
                ArabicName = "ماركة تحديث"
            }, autoSave: true);

            var created = await _cameraModelAppService.CreateAsync(new CreateUpdateCameraModelDto
            {
                EnglishName = "OldModel",
                ArabicName = "موديل قديم",
                CameraBrandId = brand.Id
            });

            var input = new CreateUpdateCameraModelDto
            {
                EnglishName = "UpdatedModel",
                ArabicName = "موديل جديد",
                CameraBrandId = brand.Id
            };

            // Act
            var updated = await _cameraModelAppService.UpdateAsync(created.Id, input);

            // Assert
            updated.EnglishName.ShouldBe("UpdatedModel");
            updated.ArabicName.ShouldBe("موديل جديد");
        }

        [Fact]
        public async Task Should_Not_Update_CameraModel_With_Existing_EnglishName()
        {
            // Arrange
            var brand = await _cameraBrandRepository.InsertAsync(new CameraBrand
            {
                EnglishName = "ModelBrand5",
                ArabicName = "ماركة ٥"
            }, autoSave: true);

            var one = await _cameraModelAppService.CreateAsync(new CreateUpdateCameraModelDto
            {
                EnglishName = "ExistingModel",
                ArabicName = "موديل ١",
                CameraBrandId = brand.Id
            });

            var two = await _cameraModelAppService.CreateAsync(new CreateUpdateCameraModelDto
            {
                EnglishName = "AnotherModel",
                ArabicName = "موديل ٢",
                CameraBrandId = brand.Id
            });

            var input = new CreateUpdateCameraModelDto
            {
                EnglishName = "ExistingModel", // duplicate
                ArabicName = "موديل مختلف",
                CameraBrandId = brand.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _cameraModelAppService.UpdateAsync(two.Id, input));
        }

        [Fact]
        public async Task Should_Not_Update_CameraModel_With_Existing_ArabicName()
        {
            // Arrange
            var brand = await _cameraBrandRepository.InsertAsync(new CameraBrand
            {
                EnglishName = "ModelBrand6",
                ArabicName = "ماركة ٦"
            }, autoSave: true);

            var one = await _cameraModelAppService.CreateAsync(new CreateUpdateCameraModelDto
            {
                EnglishName = "ModelOne",
                ArabicName = "موديل مكرر",
                CameraBrandId = brand.Id
            });

            var two = await _cameraModelAppService.CreateAsync(new CreateUpdateCameraModelDto
            {
                EnglishName = "ModelTwo",
                ArabicName = "موديل ٢",
                CameraBrandId = brand.Id
            });

            var input = new CreateUpdateCameraModelDto
            {
                EnglishName = "NewEnglish",
                ArabicName = "موديل مكرر", // duplicate
                CameraBrandId = brand.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _cameraModelAppService.UpdateAsync(two.Id, input));
        }
        [Theory]
        [InlineData("", "اسم عربي")]
        [InlineData("English Name", "")]
        [InlineData("", "")]
        public async Task Should_Not_Create_Camera_Model_With_Empty_Names(string englishName, string arabicName)
        {
            // Arrange
            var brand = await _cameraBrandRepository.InsertAsync(new CameraBrand
            {
                EnglishName = "ValidationBrand",
                ArabicName = "ماركة تحقق"
            }, autoSave: true);

            // Act & Assert
            await Assert.ThrowsAsync<AbpValidationException>(async () =>
            {
                await _cameraModelAppService.CreateAsync(new CreateUpdateCameraModelDto
                {
                    EnglishName = englishName,
                    ArabicName = arabicName,
                    CameraBrandId = brand.Id // Ensure required field is included
                });
            });
        }


        [Theory]
        [InlineData("", "اسم عربي")]
        [InlineData("English Name", "")]
        [InlineData("", "")]
        public async Task Should_Not_Update_Camera_Model_With_Empty_Names(string englishName, string arabicName)
        {
            // Arrange
            var brand = await _cameraBrandRepository.InsertAsync(new CameraBrand
            {
                EnglishName = "ValidationBrandUpdate",
                ArabicName = "ماركة تحقق التحديث"
            }, autoSave: true);

            var created = await _cameraModelAppService.CreateAsync(new CreateUpdateCameraModelDto
            {
                EnglishName = "TempModel",
                ArabicName = "نموذج مؤقت",
                CameraBrandId = brand.Id
            });

            // Act & Assert
            await Assert.ThrowsAsync<AbpValidationException>(async () =>
            {
                await _cameraModelAppService.UpdateAsync(created.Id, new CreateUpdateCameraModelDto
                {
                    EnglishName = englishName,
                    ArabicName = arabicName,
                    CameraBrandId = brand.Id
                });
            });
        }

        public async Task Should_Delete_Camera_Model()
        {
            // Arrange
            var input = new CreateUpdateCameraModelDto
            {
                EnglishName = "Camera to Delete",
                ArabicName = "كاميرا للحذف"
            };

            var created = await _cameraModelAppService.CreateAsync(input);

            // Act: Delete the camera model
            await _cameraModelAppService.DeleteAsync(created.Id);

            // Assert: Try to update the deleted camera model
            await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
            {
                await _cameraModelAppService.UpdateAsync(created.Id, input);
            });

            // Optionally, Assert that deleting results in the model being removed
            var cameraModelAfterDelete = await _cameraModelAppService.GetAsync(created.Id);
            Assert.Null(cameraModelAfterDelete);
        }


    }
}
