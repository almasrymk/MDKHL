using System;
using System.Threading.Tasks;
using FluentValidation;
using SmartParking.Discounts;
using SmartParking.Enums;
using Volo.Abp.Modularity;
using Xunit;

namespace SmartParking.Discounts
{
    public abstract class DiscountAppService_Tests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
        where TStartupModule : IAbpModule
    {
        private readonly IDiscountAppService _discountAppService;

        public DiscountAppService_Tests()
        {
            _discountAppService = GetRequiredService<IDiscountAppService>();
        }

        [Fact]
        public async Task Validate_With_ValidData_Should_Pass()
        {
            var discountDto = new CreateUpdateDiscountDto
            {
                ArabicName = "خصم تجريبي",
                EnglishName = "Test Discount",
                DiscountPercent = 20m,
                Status = Status.Active
            };

            var result = await _discountAppService.CreateAsync(discountDto);

            Assert.NotNull(result); // Ensure result is returned
            Assert.Equal("Test Discount", result.EnglishName); // Check name is set correctly
        }

        [Fact]
        public async Task Validate_With_EmptyArabicName_Should_Fail()
        {
            var discountDto = new CreateUpdateDiscountDto
            {
                ArabicName = "",
                EnglishName = "Test Discount",
                DiscountPercent = 20m,
            };

            await Assert.ThrowsAsync<ValidationException>(() => _discountAppService.CreateAsync(discountDto));
        }

        [Fact]
        public async Task Validate_With_InvalidArabicName_Should_Fail()
        {
            var discountDto = new CreateUpdateDiscountDto
            {
                ArabicName = "Test123",
                EnglishName = "Test Discount",
                DiscountPercent = 20m,
            };

            await Assert.ThrowsAsync<ValidationException>(() => _discountAppService.CreateAsync(discountDto));
        }

        [Fact]
        public async Task Validate_With_EmptyEnglishName_Should_Fail()
        {
            var discountDto = new CreateUpdateDiscountDto
            {
                ArabicName = "خصم تجريبي",
                EnglishName = "",
                DiscountPercent = 20m,
            };

            await Assert.ThrowsAsync<ValidationException>(() => _discountAppService.CreateAsync(discountDto));
        }

        [Fact]
        public async Task Validate_With_NegativeDiscountPercent_Should_Fail()
        {
            var discountDto = new CreateUpdateDiscountDto
            {
                ArabicName = "خصم تجريبي",
                EnglishName = "Test Discount",
                DiscountPercent = -5m,
            };

            await Assert.ThrowsAsync<ValidationException>(() => _discountAppService.CreateAsync(discountDto));
        }

        [Fact]
        public async Task Validate_With_DiscountPercentGreaterThan100_Should_Fail()
        {
            var discountDto = new CreateUpdateDiscountDto
            {
                ArabicName = "خصم تجريبي",
                EnglishName = "Test Discount",
                DiscountPercent = 105m,
            };

            await Assert.ThrowsAsync<ValidationException>(() => _discountAppService.CreateAsync(discountDto));
        }

        [Fact]
        public async Task Validate_With_ZeroDiscount_Should_Fail()
        {
            var discountDto = new CreateUpdateDiscountDto
            {
                ArabicName = "خصم تجريبي",
                EnglishName = "Zero Discount",
                DiscountPercent = 0m,
            };

            await Assert.ThrowsAsync<ValidationException>(() => _discountAppService.CreateAsync(discountDto));

        }

        [Fact]
        public async Task Validate_With_100PercentDiscount_Should_Fail()
        {
            var discountDto = new CreateUpdateDiscountDto
            {
                ArabicName = "خصم تجريبي",
                EnglishName = "Full Discount",
                DiscountPercent = 100m,
                
            };

            await Assert.ThrowsAsync<ValidationException>(() => _discountAppService.CreateAsync(discountDto));

        }

        [Fact]
        public async Task Validate_With_InvalidStatus_Should_Fail()
        {
            var discountDto = new CreateUpdateDiscountDto
            {
                ArabicName = "خصم تجريبي",
                EnglishName = "Test Discount",
                DiscountPercent = 50m,
            };

            await Assert.ThrowsAsync<ValidationException>(() => _discountAppService.CreateAsync(discountDto));
        }
    }
}
