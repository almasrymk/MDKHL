using System;
using System.Threading.Tasks;
using FluentValidation;
using Shouldly;
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
                Status = Status.Active,
                Notes ="This discount is test"
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

            await Assert.ThrowsAsync<Volo.Abp.Validation.AbpValidationException>(() => _discountAppService.CreateAsync(discountDto));
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

            await Assert.ThrowsAsync<Volo.Abp.Validation.AbpValidationException>(() => _discountAppService.CreateAsync(discountDto));
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

            await Assert.ThrowsAsync<Volo.Abp.Validation.AbpValidationException>(() => _discountAppService.CreateAsync(discountDto));
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

            await Assert.ThrowsAsync<Volo.Abp.Validation.AbpValidationException>(() => _discountAppService.CreateAsync(discountDto));
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

            await Assert.ThrowsAsync<Volo.Abp.Validation.AbpValidationException>(() => _discountAppService.CreateAsync(discountDto));
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

            await Assert.ThrowsAsync<Volo.Abp.Validation.AbpValidationException>(() => _discountAppService.CreateAsync(discountDto));

        }

        [Fact]
        public async Task Validate_With_100PercentDiscount_Should_Success()
        {
            var discountDto = new CreateUpdateDiscountDto
            {
                ArabicName = "خصم تجريبي",
                EnglishName = "Full Discount",
                DiscountPercent = 100m,
                
            };

            var result = await _discountAppService.CreateAsync(discountDto);

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

            await Assert.ThrowsAsync<Volo.Abp.Validation.AbpValidationException>(() => _discountAppService.CreateAsync(discountDto));
        }
    

     [Fact]
        public async Task CreateDiscount()
        {

            var discount = await _discountAppService.CreateAsync(new CreateUpdateDiscountDto
            {
                ArabicName = "خصم الشتاء",
                EnglishName = "Winter Discount",
                DiscountPercent = 12.15M,
                Status = Enums.Status.Inactive,
                Notes = "Winter promotion",
                TenantId = new Guid("3a177b65-6973-9fcf-cc7d-e988aea9085e")
            });

            Assert.NotNull(discount);
        }


        [Fact]
        public async Task Should_Edit_Discount_Successfully()
        {
            var discount = await _discountAppService.CreateAsync(new CreateUpdateDiscountDto
            {
                ArabicName = "خصم الشتاء",
                EnglishName = "Winter Discount",
                DiscountPercent = 12.15m,
                Notes = "Winter promotion",
            });

            var updateDto = new CreateUpdateDiscountDto
            {
                ArabicName = "خصم الشتاء",
                EnglishName = "Winter Discount",
                DiscountPercent = 12.15M,
                Status = Enums.Status.Active,
                Notes = "Winter promotion",
            };
            discount.DiscountPercent = 12.15m;
            var result = await _discountAppService.UpdateAsync(discount.Id, updateDto);
            Assert.NotNull(result);
            //await Assert.ThrowsAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(() => _discountAppService.UpdateAsync(discount.Id, updateDto));
        }

        [Fact]
        public async Task Should_Not_Edit_NonExisting_Discount()
        {
            var updateDto = new CreateUpdateDiscountDto
            {
                ArabicName = "خصم الشتاء",
                EnglishName = "Winter Discount",
                DiscountPercent = 10.15M,
                Status = Status.Active,
                Notes = "Winter promotion"
            };

            await Assert.ThrowsAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(async () => await _discountAppService.UpdateAsync(9999, updateDto));
        }

        [Fact]
        public async Task Should_Not_Edit_Discount_With_Empty_ArabicName()
        {
            var discount = await _discountAppService.CreateAsync(new CreateUpdateDiscountDto
            {
                ArabicName = "خصم",
                EnglishName = "Discount",
                DiscountPercent = 20.0M,
                Status = Status.Active,
                Notes = "Test discount",
                TenantId = new Guid("3a177b65-6973-9fcf-cc7d-e988aea9085e")
            });

            var updateDto = new CreateUpdateDiscountDto
            {
                ArabicName = "",
                EnglishName = "Updated Discount",
                DiscountPercent = 20.0M,
                Status = Status.Active,
                Notes = "Updated",
                TenantId = new Guid("3a177b65-6973-9fcf-cc7d-e988aea9085e")
            };
            await Assert.ThrowsAsync<Volo.Abp.Validation.AbpValidationException>(() => _discountAppService.UpdateAsync(1, updateDto));

        }

        [Fact]
        public async Task Should_Not_Edit_Discount_With_Invalid_Percentage()
        {
            var discount = await _discountAppService.CreateAsync(new CreateUpdateDiscountDto
            {
                ArabicName = "خصم رمضان",
                EnglishName = "Ramadan Discount",
                DiscountPercent = 15M,
                Status = Status.Active,
                Notes = "Ramadan offer",
                TenantId = new Guid("3a177b65-6973-9fcf-cc7d-e988aea9085e")
            });

            var updateDto = new CreateUpdateDiscountDto
            {
                ArabicName = "خصم العيد",
                EnglishName = "Eid Discount",
                DiscountPercent = -5M,
                Status = Status.Active,
                Notes = "Invalid percentage"
            };

            await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(() => _discountAppService.UpdateAsync(1, updateDto));
        }
    }
}
