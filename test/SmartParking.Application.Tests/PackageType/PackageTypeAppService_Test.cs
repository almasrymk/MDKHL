using SmartParking.Lookups;
using SmartParking.LookupsAppServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Xunit;

namespace SmartParking.PackageType;

/// <summary>
/// Unit tests for the <see cref="IPackageTypesAppService"/> service.
/// </summary>
public class PackageTypeAppService_Test<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
   where TStartupModule : IAbpModule
{
    private readonly IPackageTypesAppService _packageTypesAppService;

    public PackageTypeAppService_Test()
    {
        _packageTypesAppService = GetRequiredService<IPackageTypesAppService>();
    }

    /// <summary>
    /// Should create a valid package type when given valid input.
    /// </summary>
    [Fact]
    public async Task Should_Create_Valid_PackageType()
    {
        var input = new CreateUpdatePackageTypeDto
        {
            EnglishName = "Basic Package",
            ArabicName = "الباقة الأساسية"
        };

        var result = await _packageTypesAppService.CreateAsync(input);

        Assert.NotNull(result);
        Assert.Equal(input.EnglishName, result.EnglishName);
        Assert.Equal(input.ArabicName, result.ArabicName);
    }

    /// <summary>
    /// Should throw an exception when attempting to create a package type with a duplicate English name.
    /// </summary>
    [Fact]
    public async Task Should_Not_Allow_Duplicate_EnglishName()
    {
        var input = new CreateUpdatePackageTypeDto
        {
            EnglishName = "DuplicateName",
            ArabicName = "نسخة"
        };
        await _packageTypesAppService.CreateAsync(input);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _packageTypesAppService.CreateAsync(new CreateUpdatePackageTypeDto
            {
                EnglishName = "DuplicateName",
                ArabicName = "جديد"
            });
        });
    }

    /// <summary>
    /// Should throw an exception when attempting to create a package type with a duplicate Arabic name.
    /// </summary>
    [Fact]
    public async Task Should_Not_Allow_Duplicate_ArabicName()
    {
        var input = new CreateUpdatePackageTypeDto
        {
            EnglishName = "UniqueName",
            ArabicName = "اسممكرر"
        };
        await _packageTypesAppService.CreateAsync(input);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _packageTypesAppService.CreateAsync(new CreateUpdatePackageTypeDto
            {
                EnglishName = "NewName",
                ArabicName = "اسممكرر"
            });
        });
    }

    /// <summary>
    /// Should update an existing package type successfully with new values.
    /// </summary>
    [Fact]
    public async Task Should_Update_PackageType()
    {
        var entity = await _packageTypesAppService.CreateAsync(new CreateUpdatePackageTypeDto
        {
            EnglishName = "Old Name",
            ArabicName = "اسم قديم"
        });

        var updated = await _packageTypesAppService.UpdateAsync(entity.Id, new CreateUpdatePackageTypeDto
        {
            EnglishName = "New Name",
            ArabicName = "اسم جديد"
        });

        Assert.Equal("New Name", updated.EnglishName);
        Assert.Equal("اسم جديد", updated.ArabicName);
    }

    /// <summary>
    /// Should throw an exception when trying to update a non-existent package type.
    /// </summary>
    [Fact]
    public async Task Should_Throw_When_Updating_Nonexistent_Id()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
        {
            await _packageTypesAppService.UpdateAsync(99999, new CreateUpdatePackageTypeDto
            {
                EnglishName = "Test",
                ArabicName = "تجربة"
            });
        });
    }

    /// <summary>
    /// Should delete an existing package type and verify it no longer exists.
    /// </summary>
    [Fact]
    public async Task Should_Delete_PackageType()
    {
        var entity = await _packageTypesAppService.CreateAsync(new CreateUpdatePackageTypeDto
        {
            EnglishName = "Delete Me",
            ArabicName = "احذف"
        });

        Assert.Equal(1, entity.Id);

        await _packageTypesAppService.DeleteAsync(entity.Id);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _packageTypesAppService.GetAsync(entity.Id));
    }

    /// <summary>
    /// Should return a list of package types with pagination.
    /// </summary>
    [Fact]
    public async Task Should_Get_List()
    {
        var result = await _packageTypesAppService.GetListAsync(new PagedAndSortedResultRequestDto
        {
            MaxResultCount = 10,
            SkipCount = 0
        });

        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    /// <summary>
    /// Should return package types that match the search term in English name.
    /// </summary>
    [Fact]
    public async Task Should_Search_By_EnglishName()
    {
        await _packageTypesAppService.CreateAsync(new CreateUpdatePackageTypeDto
        {
            EnglishName = "SearchTest",
            ArabicName = "بحث"
        });

        var result = await _packageTypesAppService.Search(new PagedAndSortedAndSearchResultRequestDto
        {
            SearchingTerm = "SearchTest",
            MaxResultCount = 10,
            SkipCount = 0
        });

        Assert.NotNull(result);
        Assert.True(result.TotalCount > 0);
    }

    /// <summary>
    /// Should return all package types in the system.
    /// </summary>
    [Fact]
    public async Task Should_Get_All_List()
    {
        var result = await _packageTypesAppService.GetAllListAsync();

        Assert.NotNull(result);
    }
}