using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Volo.Abp.Application.Dtos;
using System.Threading.Tasks;
using Volo.Abp.Modularity;
using Xunit;
using Shouldly;

namespace SmartParking.SubscriptionTypes;
/// <summary>
/// Unit tests for SubscriptionTypeAppService.
/// </summary>
/// <typeparam name="TStartupModule">The startup module type.</typeparam>
public class SubscriptionTypeAppServiceTests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly ISubscriptionTypeAppService _subscriptionTypeAppService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionTypeAppServiceTests{TStartupModule}"/> class.
    /// </summary>
    public SubscriptionTypeAppServiceTests()
    {
        _subscriptionTypeAppService = GetRequiredService<ISubscriptionTypeAppService>();
    }

    /// <summary>
    /// Verifies that the list of subscription types is not empty and contains expected count.
    /// </summary>
    [Fact]
    public async Task GetListAsync()
    {
        var result = await _subscriptionTypeAppService.GetListAsync(new PagedAndSortedResultRequestDto { MaxResultCount = 100, SkipCount = 0 });
        Assert.NotEmpty(result.Items);
        Assert.Equal(3, result.TotalCount);
    }

    /// <summary>
    /// Should create a valid subscription type successfully.
    /// </summary>
    [Fact]
    public async Task Should_Create_A_Valid_SubscriptionType()
    {
        var input = new CreateUpdateSubscriptionTypeDto
        {
            TypeEn = "Basic Plan",
            TypeAr = "الخطة الأساسية"
        };

        var result = await _subscriptionTypeAppService.CreateAsync(input);
        Assert.NotNull(result);
        Assert.Equal("Basic Plan", result.TypeEn);
    }

    /// <summary>
    /// Should not allow creation of duplicate subscription types.
    /// </summary>
    [Fact]
    public async Task Should_Not_Allow_Duplicate_SubscriptionType()
    {
        var input = new CreateUpdateSubscriptionTypeDto
        {
            TypeEn = "Dubplicates",
            TypeAr = "خطة مكررة"
        };

        await _subscriptionTypeAppService.CreateAsync(input);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _subscriptionTypeAppService.CreateAsync(input);
        });
    }

    /// <summary>
    /// Should update a subscription type successfully.
    /// </summary>
    [Fact]
    public async Task Should_Update_SubscriptionType_Successfully()
    {
        var createInput = new CreateUpdateSubscriptionTypeDto { TypeEn = "Starter", TypeAr = "مبتدئ" };
        var created = await _subscriptionTypeAppService.CreateAsync(createInput);

        var updateInput = new CreateUpdateSubscriptionTypeDto { TypeEn = "Pro", TypeAr = "محترف" };

        var result = await _subscriptionTypeAppService.UpdateAsync(created.Id, updateInput);

        result.TypeEn.ShouldBe("Pro");
    }

    /// <summary>
    /// Should delete a subscription type and confirm it's removed from the list.
    /// </summary>
    [Fact]
    public async Task Should_Delete_SubscriptionType()
    {
        var input = new CreateUpdateSubscriptionTypeDto { TypeEn = "Temp", TypeAr = "مؤقت" };
        var created = await _subscriptionTypeAppService.CreateAsync(input);

        await _subscriptionTypeAppService.DeleteAsync(created.Id);

        var all = await _subscriptionTypeAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        all.Items.Any(x => x.Id == created.Id).ShouldBeFalse();
    }

    /// <summary>
    /// Should retrieve subscription types for lookup usage.
    /// </summary>
    [Fact]
    public async Task Should_Get_SubscriptionType_Lookup_List()
    {
        var input = new CreateUpdateSubscriptionTypeDto { TypeEn = "LookupPlan", TypeAr = "خطة العرض" };
        await _subscriptionTypeAppService.CreateAsync(input);

        var result = await _subscriptionTypeAppService.GetSubscriptionTypeLookupAsync();

        Assert.Equal("LookupPlan", result[3].SubscriptionTypeEn);
    }

    /// <summary>
    /// Should return subscription types matching the search term.
    /// </summary>
    [Fact]
    public async Task Should_Search_SubscriptionTypes_By_Term()
    {
        await _subscriptionTypeAppService.CreateAsync(new CreateUpdateSubscriptionTypeDto
        {
            TypeEn = "Enterprise",
            TypeAr = "مؤسسة"
        });

        var result = await _subscriptionTypeAppService.SearchForMDKHL(new PagedAndSortedAndSearchResultRequestDto
        {
            SearchingTerm = "Enter",
            SkipCount = 0,
            MaxResultCount = 10
        });

        result.Items.ShouldContain(x => x.TypeEn.Contains("Enter"));
    }
}

