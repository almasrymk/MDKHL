using Shouldly;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace SmartParking.Subscriptions;
/// <summary>
///  class for testing SubscriptionAppService  when try to create , update , delete or get by id  
/// </summary>
public class SubscriptionAppServiceTest<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
   where TStartupModule : IAbpModule
{
    private readonly ISubscriptionAppService _subscriptionAppService;

    public SubscriptionAppServiceTest()
    {
        _subscriptionAppService = GetRequiredService<ISubscriptionAppService>();
    }

    /// <summary>
    /// Verifies that subscriptions seeded in the database are returned and not null.
    /// </summary>
    [Fact]
    public async Task Should_Get_Subscriptions_From_Seed_NotNull()
    {
        var result = await _subscriptionAppService.GetListAsync(new PagedAndSortedResultRequestDto { MaxResultCount = 100, SkipCount = 0 });
        Assert.NotEmpty(result.Items);
        Assert.Equal(1, result.TotalCount);
    }

    /// <summary>
    /// Should create a subscription successfully with valid input.
    /// </summary>
    [Fact]
    public async Task Should_Create_Subscription_Successfully()
    {
        var input = new CreateUpdateSubscriptionDto
        {
            ArabicName = "الخطة المميزة",
            EnglishName = "Premium Plan",
            SubscriptionPrice = 49.99m,
            SubscriptionDescription = "Access to all features with no limits.",
            ActivationDate = new DateOnly(2024, 10, 1),
            InActivationDate = new DateOnly(2025, 10, 1),
            NumbersOfGarages = 5,
            NumberOfCameras = 10,
            NumberOfUsers = 100,
            NumberOfCustomers = 50,
            SubscriptionSlotNumber = 10,
            SubscriptionTypeId = 2,
            ParkingSlot = 1000,
            Status = SubscriptionStatus.Draft
        };

        var created = await _subscriptionAppService.CreateAsync(input);
        Assert.NotEqual(0, created.Id);
    }

    /// <summary>
    /// Should fail when required EnglishName is missing.
    /// </summary>
    [Fact]
    public async Task Should_Create_Subscription_Required_Fails()
    {
        var input = new CreateUpdateSubscriptionDto
        {
            ArabicName = "الخطة المميزة",
            SubscriptionPrice = 49.99m,
            SubscriptionDescription = "Access to all features with no limits.",
            ActivationDate = new DateOnly(2024, 10, 1),
            InActivationDate = new DateOnly(2025, 10, 1),
            NumbersOfGarages = 5,
            NumberOfCameras = 10,
            NumberOfUsers = 100,
            NumberOfCustomers = 50,
            SubscriptionSlotNumber = 10,
            SubscriptionTypeId = 2,
            ParkingSlot = 1000,
            Status = SubscriptionStatus.Draft
        };

        await Assert.ThrowsAsync<AbpValidationException>(() => _subscriptionAppService.CreateAsync(input));
    }

    /// <summary>
    /// Should throw validation exception if invalid enum status is provided.
    /// </summary>
    [Fact]
    public async Task Should_Create_Subscription_Invalid_Enum_Status()
    {
        var input = new CreateUpdateSubscriptionDto
        {
            ArabicName = "الخطة المميزة",
            SubscriptionPrice = 49.99m,
            SubscriptionDescription = "Access to all features with no limits.",
            ActivationDate = new DateOnly(2024, 10, 1),
            InActivationDate = new DateOnly(2025, 10, 1),
            NumbersOfGarages = 5,
            NumberOfCameras = 10,
            NumberOfUsers = 100,
            NumberOfCustomers = 50,
            SubscriptionSlotNumber = 10,
            SubscriptionTypeId = 2,
            ParkingSlot = 1000,
            Status = (SubscriptionStatus)5
        };

        await Assert.ThrowsAsync<AbpValidationException>(() => _subscriptionAppService.CreateAsync(input));
    }

    /// <summary>
    /// Should update the subscription status from Draft to Active.
    /// </summary>
    [Fact]
    public async Task Should_Update_Subscription_Status_From_Draft_To_Active()
    {
        var input = new CreateUpdateSubscriptionDto
        {
            ArabicName = "الخطة المميزة",
            EnglishName = "Premium Plan",
            SubscriptionPrice = 49.99m,
            SubscriptionDescription = "Access to all features with no limits.",
            ActivationDate = new DateOnly(2024, 10, 1),
            InActivationDate = new DateOnly(2025, 10, 1),
            NumbersOfGarages = 5,
            NumberOfCameras = 10,
            NumberOfUsers = 100,
            NumberOfCustomers = 50,
            SubscriptionSlotNumber = 10,
            SubscriptionTypeId = 2,
            ParkingSlot = 1000,
            Status = SubscriptionStatus.Draft
        };

        var created = await _subscriptionAppService.CreateAsync(input);
        input.Status = SubscriptionStatus.Active;

        var result = await _subscriptionAppService.UpdateAsync(created.Id, input);
        Assert.Equal(SubscriptionStatus.Active, result.Status);
    }

    /// <summary>
    /// Should not allow status change back to Draft once it has been set to Active.
    /// </summary>
    [Fact]
    public async Task Should_Throw_When_Changing_Status_Back_To_Draft()
    {
        var input = new CreateUpdateSubscriptionDto
        {
            ArabicName = "الخطة المميزة",
            EnglishName = "Premium Plan",
            SubscriptionPrice = 49.99m,
            SubscriptionDescription = "Access to all features with no limits.",
            ActivationDate = new DateOnly(2024, 10, 1),
            InActivationDate = new DateOnly(2025, 10, 1),
            NumbersOfGarages = 5,
            NumberOfCameras = 10,
            NumberOfUsers = 100,
            NumberOfCustomers = 50,
            SubscriptionSlotNumber = 10,
            SubscriptionTypeId = 1,
            ParkingSlot = 1000,
            Status = SubscriptionStatus.Active
        };

        var created = await _subscriptionAppService.CreateAsync(input);
        input.Status = SubscriptionStatus.Active;
        await _subscriptionAppService.UpdateAsync(created.Id, input);

        input.Status = SubscriptionStatus.Draft;

        await Assert.ThrowsAsync<InvalidOperationException>(() => _subscriptionAppService.UpdateAsync(created.Id, input));
    }

    /// <summary>
    /// Should retrieve a created subscription by its ID.
    /// </summary>
    [Fact]
    public async Task Should_Get_Subscription_By_Id()
    {
        var input = new CreateUpdateSubscriptionDto
        {
            ArabicName = "الخطة المميزة",
            EnglishName = "Premium Plan",
            SubscriptionPrice = 49.99m,
            SubscriptionDescription = "Access to all features with no limits.",
            ActivationDate = new DateOnly(2024, 10, 1),
            InActivationDate = new DateOnly(2025, 10, 1),
            NumbersOfGarages = 5,
            NumberOfCameras = 10,
            NumberOfUsers = 100,
            NumberOfCustomers = 50,
            SubscriptionSlotNumber = 10,
            SubscriptionTypeId = 1,
            ParkingSlot = 1000,
            Status = SubscriptionStatus.Active
        };

        var created = await _subscriptionAppService.CreateAsync(input);
        var result = await _subscriptionAppService.GetAsync(created.Id);

        Assert.NotNull(result);
        Assert.Equal(SubscriptionStatus.Draft, result.Status); // assuming service overrides status on creation
    }

    /// <summary>
    /// Should delete a subscription and confirm it's no longer retrievable.
    /// </summary>
    [Fact]
    public async Task Should_Delete_Subscription()
    {
        var input = new CreateUpdateSubscriptionDto
        {
            ArabicName = "الخطة المميزة",
            EnglishName = "Premium Plan",
            SubscriptionPrice = 49.99m,
            SubscriptionDescription = "Access to all features with no limits.",
            ActivationDate = new DateOnly(2024, 10, 1),
            InActivationDate = new DateOnly(2025, 10, 1),
            NumbersOfGarages = 5,
            NumberOfCameras = 10,
            NumberOfUsers = 100,
            NumberOfCustomers = 50,
            SubscriptionSlotNumber = 10,
            SubscriptionTypeId = 1,
            ParkingSlot = 1000,
            Status = SubscriptionStatus.Active
        };

        var created = await _subscriptionAppService.CreateAsync(input);
        await _subscriptionAppService.DeleteAsync(created.Id);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _subscriptionAppService.GetAsync(created.Id));
    }
}


