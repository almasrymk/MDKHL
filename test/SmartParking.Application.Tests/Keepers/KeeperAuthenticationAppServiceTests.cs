using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Shouldly;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Mobile.Keepers;
using SmartParking.Permissions.CheckoutPermissions;
using SmartParking.Permissions.UnlinkedEntryPermissions;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;
using Xunit;
using IdentityUser = Volo.Abp.Identity.IdentityUser;
using IdentityRole = Volo.Abp.Identity.IdentityRole;

namespace SmartParking.Keepers;

public abstract class KeeperAuthenticationAppServiceTests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    protected readonly IKeeperAuthenticationAppService _keeperAuthAppService;
    protected readonly IdentityUserManager _userManager;
    protected readonly IRepository<IdentityUser, Guid> _userRepository;
    protected readonly IRepository<IdentityRole, Guid> _roleRepository;
    protected readonly IdentityRoleManager _roleManager;
    protected readonly IRepository<Organization, int> _organizationRepository;
    protected readonly IPermissionManager _permissionManager;
    protected readonly IDataFilter _dataFilter;
    protected readonly IUnitOfWorkManager _unitOfWorkManager;

    protected KeeperAuthenticationAppServiceTests()
    {
        _keeperAuthAppService = GetRequiredService<IKeeperAuthenticationAppService>();
        _userManager = GetRequiredService<IdentityUserManager>();
        _roleManager = GetRequiredService<IdentityRoleManager>();
        _userRepository = GetRequiredService<IRepository<IdentityUser, Guid>>();
        _roleRepository = GetRequiredService<IRepository<IdentityRole, Guid>>();
        _organizationRepository = GetRequiredService<IRepository<Organization, int>>();
        _permissionManager = GetRequiredService<IPermissionManager>();
        _dataFilter = GetRequiredService<IDataFilter>();
        _unitOfWorkManager = GetRequiredService<IUnitOfWorkManager>();
    }

    private async Task<(IdentityUser user, string password, Organization org)> CreateTestKeeperAsync(string suffix)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var org = (await _organizationRepository.GetListAsync()).First();

            var userId = Guid.NewGuid();
            var phone = "055" + suffix.PadLeft(7, '0').Substring(0, 7);
            var user = new IdentityUser(userId, "keeper_" + suffix, $"keeper_{suffix}@example.com", org.TenantId);
            user.SetPhoneNumber(phone, true);
            user.SetProperty("OrganizationId", org.Id);
            user.SetProperty("UserTypeCategory", UserTypeCategory.Keeper);

            var password = "P@ssw0rd123!";
            await _userManager.CreateAsync(user, password);

            return (user, password, org);
        }
    }

    [Fact]
    public async Task LoginAsync_Should_Return_True_When_Keeper_Has_UnlinkedEntry_Permission()
    {
        var (user, password, org) = await CreateTestKeeperAsync("101");

        var roleId = Guid.NewGuid();
        var roleName = "KeeperRole_WithPerm_" + roleId.ToString("N").Substring(0, 6);

        using (_dataFilter.Disable<IMultiTenant>())
        {
            // Create role and assign permission
            var role = new IdentityRole(roleId, roleName, org.TenantId);
            await _roleRepository.InsertAsync(role, autoSave: true);

            user.AddRole(roleId);
            await _userManager.UpdateAsync(user);

            // Grant permission to the role
            await _permissionManager.SetForRoleAsync(roleName, UnlinkedEntryPermissions.Default, true);
        }

        // Act
        var loginResult = await _keeperAuthAppService.LoginAsync(new LoginRequestDto
        {
            PhoneNumber = user.PhoneNumber!,
            Password = password
        });

        // Assert
        loginResult.ShouldNotBeNull();
        loginResult.IsSuccess.ShouldBeTrue();
        loginResult.Value.UnlinkedEntryTransaction.ShouldBeTrue();
    }

    [Fact]
    public async Task LoginAsync_Should_Return_False_When_Keeper_Does_Not_Have_UnlinkedEntry_Permission()
    {
        var (user, password, org) = await CreateTestKeeperAsync("102");

        using (_dataFilter.Disable<IMultiTenant>())
        {
            var roleId = Guid.NewGuid();
            var roleName = "KeeperRole_NoPerm_" + roleId.ToString("N").Substring(0, 6);
            var role = new IdentityRole(roleId, roleName, org.TenantId);
            await _roleRepository.InsertAsync(role, autoSave: true);

            user.AddRole(roleId);
            await _userManager.UpdateAsync(user);
        }

        // Act
        var loginResult = await _keeperAuthAppService.LoginAsync(new LoginRequestDto
        {
            PhoneNumber = user.PhoneNumber!,
            Password = password
        });

        // Assert
        loginResult.ShouldNotBeNull();
        loginResult.IsSuccess.ShouldBeTrue();
        loginResult.Value.ShouldNotBeNull();
        loginResult.Value.UnlinkedEntryTransaction.ShouldBeFalse();
    }



    [Fact]
    public async Task LoginAsync_Should_Correctly_Evaluate_Different_Permissions_For_Different_Keepers()
    {
        // Keeper A has UnlinkedEntry permission, Keeper B does not
        var (userA, passwordA, orgA) = await CreateTestKeeperAsync("103");
        var (userB, passwordB, orgB) = await CreateTestKeeperAsync("104");

        using (_dataFilter.Disable<IMultiTenant>())
        {
            var roleA = new IdentityRole(Guid.NewGuid(), "RoleA_" + Guid.NewGuid().ToString("N").Substring(0, 6), orgA.TenantId);
            await _roleRepository.InsertAsync(roleA, autoSave: true);
            userA.AddRole(roleA.Id);
            await _userManager.UpdateAsync(userA);
            await _permissionManager.SetForRoleAsync(roleA.Name, UnlinkedEntryPermissions.Default, true);

            var roleB = new IdentityRole(Guid.NewGuid(), "RoleB_" + Guid.NewGuid().ToString("N").Substring(0, 6), orgB.TenantId);
            await _roleRepository.InsertAsync(roleB, autoSave: true);
            userB.AddRole(roleB.Id);
            await _userManager.UpdateAsync(userB);
        }

        // Act
        var resultA = await _keeperAuthAppService.LoginAsync(new LoginRequestDto
        {
            PhoneNumber = userA.PhoneNumber!,
            Password = passwordA
        });

        var resultB = await _keeperAuthAppService.LoginAsync(new LoginRequestDto
        {
            PhoneNumber = userB.PhoneNumber!,
            Password = passwordB
        });

        // Assert
        resultA.IsSuccess.ShouldBeTrue();
        resultA.Value.UnlinkedEntryTransaction.ShouldBeTrue();

        resultB.IsSuccess.ShouldBeTrue();
        resultB.Value.UnlinkedEntryTransaction.ShouldBeFalse();
    }

    [Fact]
    public async Task LoginAsync_Should_Evaluate_CanCancelTransaction_Correctly()
    {
        var (user, password, org) = await CreateTestKeeperAsync("105");

        using (_dataFilter.Disable<IMultiTenant>())
        {
            var roleId = Guid.NewGuid();
            var roleName = "KeeperRole_Cancel_" + roleId.ToString("N").Substring(0, 6);
            var role = new IdentityRole(roleId, roleName, org.TenantId);
            await _roleRepository.InsertAsync(role, autoSave: true);

            user.AddRole(roleId);
            await _userManager.UpdateAsync(user);

            await _permissionManager.SetForRoleAsync(roleName, CheckoutPermissions.CancelTransaction, true);
        }

        // Act
        var loginResult = await _keeperAuthAppService.LoginAsync(new LoginRequestDto
        {
            PhoneNumber = user.PhoneNumber!,
            Password = password
        });

        // Assert
        loginResult.ShouldNotBeNull();
        loginResult.IsSuccess.ShouldBeTrue();
        loginResult.Value.CanCancelTransaction.ShouldBeTrue();
    }

    [Fact]
    public async Task GenerateTokenAsync_Should_Include_Abp_Standard_Claims_And_Individual_Roles()
    {
        var authUtility = GetRequiredService<SmartParking.Utilities.IAuthenticationUtility>();

        string role1Name = "RoleOne_" + Guid.NewGuid().ToString("N").Substring(0, 6);
        string role2Name = "RoleTwo_" + Guid.NewGuid().ToString("N").Substring(0, 6);

        IdentityUser user;
        Organization org;

        using (var uow = _unitOfWorkManager.Begin())
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                var created = await CreateTestKeeperAsync("201");
                user = created.user;
                org = created.org;

                var role1 = new IdentityRole(Guid.NewGuid(), role1Name, org.TenantId);
                var role2 = new IdentityRole(Guid.NewGuid(), role2Name, org.TenantId);
                (await _roleManager.CreateAsync(role1)).Succeeded.ShouldBeTrue();
                (await _roleManager.CreateAsync(role2)).Succeeded.ShouldBeTrue();

                (await _userManager.AddToRoleAsync(user, role1Name)).Succeeded.ShouldBeTrue();
                (await _userManager.AddToRoleAsync(user, role2Name)).Succeeded.ShouldBeTrue();
            }
            await uow.CompleteAsync();
        }

        var token = await authUtility.GenerateTokenAsync(user);
        token.ShouldNotBeNullOrEmpty();

        var handler = new Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(token);

        // Verify standard ABP user identification claims
        jwt.Claims.ShouldContain(c => c.Type == "sub" && c.Value == user.Id.ToString());
        jwt.Claims.ShouldContain(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
        jwt.Claims.ShouldContain(c => c.Type == Volo.Abp.Security.Claims.AbpClaimTypes.UserId && c.Value == user.Id.ToString());

        // Verify standard ABP username claims
        jwt.Claims.ShouldContain(c => c.Type == "name" && c.Value == user.UserName);
        jwt.Claims.ShouldContain(c => c.Type == System.Security.Claims.ClaimTypes.Name && c.Value == user.UserName);
        jwt.Claims.ShouldContain(c => c.Type == Volo.Abp.Security.Claims.AbpClaimTypes.UserName && c.Value == user.UserName);

        // Verify individual role claims (both standard "role" and ClaimTypes.Role for ABP RolePermissionValueProvider)
        jwt.Claims.ShouldContain(c => c.Type == "role" && c.Value == role1Name);
        jwt.Claims.ShouldContain(c => c.Type == "role" && c.Value == role2Name);
        jwt.Claims.ShouldContain(c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == role1Name);
        jwt.Claims.ShouldContain(c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == role2Name);
        jwt.Claims.ShouldContain(c => c.Type == Volo.Abp.Security.Claims.AbpClaimTypes.Role && c.Value == role1Name);
        jwt.Claims.ShouldContain(c => c.Type == Volo.Abp.Security.Claims.AbpClaimTypes.Role && c.Value == role2Name);

        // Verify tenant claim
        if (org.TenantId.HasValue)
        {
            jwt.Claims.ShouldContain(c => c.Type == "tenantid" && c.Value == org.TenantId.ToString());
            jwt.Claims.ShouldContain(c => c.Type == Volo.Abp.Security.Claims.AbpClaimTypes.TenantId && c.Value == org.TenantId.ToString());
        }
        else
        {
            jwt.Claims.Any(c => c.Type == "tenantid").ShouldBeFalse();
        }
    }

    [Fact]
    public async Task GenerateTokenAsync_Should_Include_TenantId_Claim_When_User_Belongs_To_Tenant()
    {
        var authUtility = GetRequiredService<SmartParking.Utilities.IAuthenticationUtility>();
        var tenantId = Guid.NewGuid();

        IdentityUser tenantUser;
        using (var uow = _unitOfWorkManager.Begin())
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                var userId = Guid.NewGuid();
                var suffix = Guid.NewGuid().ToString("N").Substring(0, 6);
                tenantUser = new IdentityUser(userId, "tkeeper_" + suffix, $"tkeeper_{suffix}@example.com", tenantId);
                tenantUser.SetPhoneNumber("055" + suffix.PadLeft(7, '1').Substring(0, 7), true);
                (await _userManager.CreateAsync(tenantUser, "P@ssw0rd123!")).Succeeded.ShouldBeTrue();
            }
            await uow.CompleteAsync();
        }

        var token = await authUtility.GenerateTokenAsync(tenantUser);
        token.ShouldNotBeNullOrEmpty();

        var handler = new Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(token);

        jwt.Claims.ShouldContain(c => c.Type == "tenantid" && c.Value == tenantId.ToString());
        jwt.Claims.ShouldContain(c => c.Type == Volo.Abp.Security.Claims.AbpClaimTypes.TenantId && c.Value == tenantId.ToString());
    }
}
