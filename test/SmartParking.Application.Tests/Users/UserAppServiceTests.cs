using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Xunit;
using SmartParking.Authentication;
using SmartParking.Entities.Organizations;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Users;

public abstract class UserAppServiceTests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
   where TStartupModule : IAbpModule
{
    protected readonly IUsersAppService _usersAppService;
    protected readonly IRoleAppService _roleAppService;
    protected readonly IRepository<IdentityUser, Guid> _userRepository;
    protected readonly IRepository<IdentityRole, Guid> _roleRepository;
    protected readonly IRepository<IdentityUserRole> _userRoleRepository;
    protected readonly IRepository<Organization, int> _organizationRepository;
    protected readonly IdentityUserManager _userManager;
    protected readonly IUnitOfWorkManager _unitOfWorkManager;
    protected readonly IDataFilter _dataFilter;

    protected UserAppServiceTests()
    {
        _usersAppService = GetRequiredService<IUsersAppService>();
        _roleAppService = GetRequiredService<IRoleAppService>();
        _userRepository = GetRequiredService<IRepository<IdentityUser, Guid>>();
        _roleRepository = GetRequiredService<IRepository<IdentityRole, Guid>>();
        _userRoleRepository = GetRequiredService<IRepository<IdentityUserRole>>();
        _organizationRepository = GetRequiredService<IRepository<Organization, int>>();
        _userManager = GetRequiredService<IdentityUserManager>();
        _unitOfWorkManager = GetRequiredService<IUnitOfWorkManager>();
        _dataFilter = GetRequiredService<IDataFilter>();
    }

    private async Task<IdentityUser> CreateTestUserAsync(Guid userId)
    {
        var user = new IdentityUser(userId, "testuser_" + userId.ToString("N").Substring(0, 6), "test_" + userId.ToString("N").Substring(0, 6) + "@example.com");
        await _userManager.CreateAsync(user, "P@ssw0rd123!");
        return user;
    }

    [Fact]
    public async Task Should_Assign_Role_And_Permissions_Successfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await CreateTestUserAsync(userId);

        var roleId = Guid.NewGuid();
        var role = new IdentityRole(roleId, "TestRole_" + roleId.ToString("N").Substring(0, 6));
        await _roleRepository.InsertAsync(role, autoSave: true);

        var tenantId = Guid.NewGuid();

        // Act
        await _usersAppService.AssignRoleAndPermissionsAsync(userId, roleId, tenantId);

        // Assert
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var userRoles = await _userRoleRepository.GetListAsync(r => r.UserId == userId);
            userRoles.Any(r => r.RoleId == roleId && r.TenantId == tenantId).ShouldBeTrue();
        }
    }

    [Fact]
    public async Task Should_Not_Create_Duplicate_Role_On_AssignRoleAndPermissionsAsync()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await CreateTestUserAsync(userId);

        var roleId = Guid.NewGuid();
        var role = new IdentityRole(roleId, "TestRole_" + roleId.ToString("N").Substring(0, 6));
        await _roleRepository.InsertAsync(role, autoSave: true);

        var tenantId = Guid.NewGuid();

        // Act
        await _usersAppService.AssignRoleAndPermissionsAsync(userId, roleId, tenantId);
        await _usersAppService.AssignRoleAndPermissionsAsync(userId, roleId, tenantId); // Duplicate call

        // Assert
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var userRoles = await _userRoleRepository.GetListAsync(r => r.UserId == userId);
            userRoles.Count(r => r.RoleId == roleId && r.TenantId == tenantId).ShouldBe(1);
        }
    }

    [Fact]
    public async Task Should_AssignRole_Without_Duplicates_In_RoleAppService()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await CreateTestUserAsync(userId);

        var roleId = Guid.NewGuid();
        var role = new IdentityRole(roleId, "TestRole_" + roleId.ToString("N").Substring(0, 6));
        await _roleRepository.InsertAsync(role, autoSave: true);

        // Act
        await _roleAppService.AssignRole(userId, roleId);
        await _roleAppService.AssignRole(userId, roleId); // Duplicate call

        // Assert
        using (_unitOfWorkManager.Begin())
        {
            var updatedUser = await _userRepository.GetAsync(userId);
            updatedUser.Roles.Count(r => r.RoleId == roleId).ShouldBe(1);
        }
    }

    [Fact]
    public async Task Should_Remove_Role_On_RemoveRoleMdkhl()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = await CreateTestUserAsync(userId);
        var roleId = Guid.NewGuid();
        var role = new IdentityRole(roleId, "TestRole_" + roleId.ToString("N").Substring(0, 6));
        await _roleRepository.InsertAsync(role, autoSave: true);

        user.AddRole(roleId);
        await _userManager.UpdateAsync(user);

        // Act
        await _usersAppService.RemoveRoleMdkhl(userId, roleId);

        // Assert
        using (_unitOfWorkManager.Begin())
        {
            var updatedUser = await _userRepository.GetAsync(userId);
            updatedUser.Roles.Any(r => r.RoleId == roleId).ShouldBeFalse();
        }
    }
}
