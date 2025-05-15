
using Microsoft.EntityFrameworkCore;
using Shouldly;
using SmartParking.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace SmartParking.Countries;

public abstract class CountryAppServiceTests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
   where TStartupModule : IAbpModule
{
    
        private readonly ICountriesAppService _countriesApp;

        public CountryAppServiceTests()
        {
            _countriesApp = GetRequiredService<ICountriesAppService>();
        }

        [Fact]
        public async Task Initial_Data_Should_Contain_Country()
        {
            // Act
            var result = await _countriesApp.GetListAsync(new PagedAndSortedResultRequestDto());

            // Assert
            result.TotalCount.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task Should_Create_A_Valid_Country()
        {
            // Act
            var result = await _countriesApp.CreateAsync(
                new CreateUpdateCountryDto
                {
                    EnglishName = "Afghanistan",
                    ArabicName = "افغانستان",
                    EnglishCapital = "Kabul",
                    ArabicCapital = "كابول"
                }
            );

            // Assert
            result.ShouldNotBeNull();
            result.EnglishName.ShouldBe("Afghanistan");
            result.ArabicName.ShouldBe("افغانستان");
        }

    

    [Fact]
        public async Task Should_Not_Create_Duplicate_EnglishName()
        {
            await _countriesApp.CreateAsync(new CreateUpdateCountryDto
            {
                EnglishName = "TestCountry",
                ArabicName = "بلد الاختبار",
                EnglishCapital = "TestCapital",
                ArabicCapital = "عاصمة الاختبار"
            });

            var duplicateInput = new CreateUpdateCountryDto
            {
                EnglishName = "TestCountry", // Duplicate
                ArabicName = "بلد آخر",
                EnglishCapital = "AnotherCapital",
                ArabicCapital = "عاصمة أخرى"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _countriesApp.CreateAsync(duplicateInput));
        }

        [Fact]
        public async Task Should_Not_Create_Duplicate_ArabicName()
        {
            await _countriesApp.CreateAsync(new CreateUpdateCountryDto
            {
                EnglishName = "UniqueEnglish",
                ArabicName = "بلد مكرر",
                EnglishCapital = "Capital1",
                ArabicCapital = "عاصمة1"
            });

            var duplicateInput = new CreateUpdateCountryDto
            {
                EnglishName = "DifferentEnglish",
                ArabicName = "بلد مكرر", // Duplicate
                EnglishCapital = "Capital2",
                ArabicCapital = "عاصمة2"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _countriesApp.CreateAsync(duplicateInput));
        }

        [Fact]
        public async Task Should_Update_Country_When_Valid()
        {
            var created = await _countriesApp.CreateAsync(new CreateUpdateCountryDto
            {
                EnglishName = "OldCountry",
                ArabicName = "البلد القديم",
                EnglishCapital = "OldCapital",
                ArabicCapital = "العاصمة القديمة"
            });

            var input = new CreateUpdateCountryDto
            {
                EnglishName = "NewCountry",
                ArabicName = "البلد الجديد",
                EnglishCapital = "NewCapital",
                ArabicCapital = "العاصمة الجديدة"
            };

            var updated = await _countriesApp.UpdateAsync(created.Id, input);

            updated.EnglishName.ShouldBe("NewCountry");
            updated.ArabicName.ShouldBe("البلد الجديد");
        }

        [Fact]
        public async Task Should_Not_Update_Country_To_Duplicate_EnglishName()
        {
            var country1 = await _countriesApp.CreateAsync(new CreateUpdateCountryDto
            {
                EnglishName = "ExistingCountry",
                ArabicName = "بلد موجود",
                EnglishCapital = "Capital",
                ArabicCapital = "العاصمة"
            });

            var country2 = await _countriesApp.CreateAsync(new CreateUpdateCountryDto
            {
                EnglishName = "OtherCountry",
                ArabicName = "بلد آخر",
                EnglishCapital = "Capital2",
                ArabicCapital = "العاصمة2"
            });

            var updateInput = new CreateUpdateCountryDto
            {
                EnglishName = "ExistingCountry", // Duplicate
                ArabicName = "بلد آخر جديد",
                EnglishCapital = "UpdatedCapital",
                ArabicCapital = "العاصمة المحدثة"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _countriesApp.UpdateAsync(country2.Id, updateInput));
        }

        [Fact]
        public async Task Should_Not_Update_Country_To_Duplicate_ArabicName()
        {
            var country1 = await _countriesApp.CreateAsync(new CreateUpdateCountryDto
            {
                EnglishName = "CountryOne",
                ArabicName = "بلد مكرر",
                EnglishCapital = "CapitalOne",
                ArabicCapital = "عاصمة1"
            });

            var country2 = await _countriesApp.CreateAsync(new CreateUpdateCountryDto
            {
                EnglishName = "CountryTwo",
                ArabicName = "بلد مختلف",
                EnglishCapital = "CapitalTwo",
                ArabicCapital = "عاصمة2"
            });

            var updateInput = new CreateUpdateCountryDto
            {
                EnglishName = "CountryTwo Updated",
                ArabicName = "بلد مكرر", // Duplicate
                EnglishCapital = "UpdatedCapital",
                ArabicCapital = "عاصمة محدثة"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _countriesApp.UpdateAsync(country2.Id, updateInput));
        }

        [Fact]
        public async Task Should_Throw_When_Updating_Nonexistent_Country()
        {
            var updateInput = new CreateUpdateCountryDto
            {
                EnglishName = "NonExistent",
                ArabicName = "غير موجود",
                EnglishCapital = "None",
                ArabicCapital = "لا شيء"
            };

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _countriesApp.UpdateAsync(999999, updateInput));
        }
    [Fact]
    public async Task Should_Delete_Country()
    {
        // Arrange
        var input = new CreateUpdateCountryDto
        {
            EnglishName = "Country To Delete",
            ArabicName = "بلد للحذف",
            EnglishCapital = "DeleteCapital",
            ArabicCapital = "عاصمة للحذف"
        };

        var created = await _countriesApp.CreateAsync(input);

        // Act
        await _countriesApp.DeleteAsync(created.Id);

        // Assert: Trying to update the deleted country should throw
        await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
        {
            await _countriesApp.UpdateAsync(created.Id, input);
        });
    }
    [Theory]
    [InlineData("", "اسم عربي")]
    [InlineData("English Name", "")]
    [InlineData("", "")]
    public async Task Should_Not_Update_Country_With_Empty_Names(string englishName, string arabicName)
    {
        // Arrange
        var created = await _countriesApp.CreateAsync(new CreateUpdateCountryDto
        {
            EnglishName = "TempCountry",
            ArabicName = "بلد مؤقت",
            EnglishCapital = "Capital",
            ArabicCapital = "عاصمة"
        });

        var input = new CreateUpdateCountryDto
        {
            EnglishName = englishName,
            ArabicName = arabicName,
            EnglishCapital = "UpdatedCapital",
            ArabicCapital = "عاصمة محدثة"
        };

        // Act & Assert
        await Assert.ThrowsAsync<AbpValidationException>(async () =>
        {
            await _countriesApp.UpdateAsync(created.Id, input);
        });
    }


}

