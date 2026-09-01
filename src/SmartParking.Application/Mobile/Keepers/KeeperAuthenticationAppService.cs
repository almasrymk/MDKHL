using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Permissions.CheckoutPermissions;
using SmartParking.Permissions.UnlinkedEntryPermissions;
using SmartParking.Results;
using SmartParking.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Mobile.Keepers;

[RemoteService(isEnabled: false)]
public class KeeperAuthenticationAppService : SmartParkingAppService, IKeeperAuthenticationAppService
{
    private readonly UserManager<Volo.Abp.Identity.IdentityUser> _userManager;
    private readonly SignInManager<Volo.Abp.Identity.IdentityUser> _signInManager;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IRepository<Volo.Abp.Identity.IdentityUser, Guid> _userRepository;
    private readonly IRepository<Organization, int> _organizationRepository;
    private readonly IAuthenticationUtility _authenticationUtility;
    private readonly Volo.Abp.Authorization.Permissions.IPermissionChecker _permissionChecker;

    public KeeperAuthenticationAppService(
        UserManager<Volo.Abp.Identity.IdentityUser> userManager,
        SignInManager<Volo.Abp.Identity.IdentityUser> signInManager,
        IHttpContextAccessor httpContext,
        IRepository<Volo.Abp.Identity.IdentityUser, Guid> userRepository,
        IRepository<Organization, int> organizationRepository,
        IAuthenticationUtility authenticationUtility,
        Volo.Abp.Authorization.Permissions.IPermissionChecker permissionChecker)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _httpContext = httpContext;
        _userRepository = userRepository;
        _organizationRepository = organizationRepository;
        _authenticationUtility = authenticationUtility;
        _permissionChecker = permissionChecker;
    }

    /// <inheritdoc/>
    public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto input)
    {
        if (input.PhoneNumber.IsNullOrWhiteSpace())
        {
            return Result.Invalid(new Results.ValidationError
            {
                ErrorMessage = "Phone Number is not valid"
            });
        }

        using (DataFilter.Disable<IMultiTenant>())
        {
            var user = await _userRepository.FirstOrDefaultAsync(u => u.PhoneNumber == input.PhoneNumber || u.UserName == input.PhoneNumber);

            if (user is null || !await _userManager.CheckPasswordAsync(user, input.Password))
            {
                return Result<LoginResponseDto>.Invalid(new Results.ValidationError
                {
                    ErrorCode = "403",
                    ErrorMessage = "Invalid credentials."
                });
            }

            if (user.IsActive == false)
            {
                return Result<LoginResponseDto>.Invalid(new Results.ValidationError
                {
                    ErrorCode = "400",
                    ErrorMessage = "Account is Inactive"
                });
            }

            var orgId = user.GetProperty<int>("OrganizationId");
            var organizationEntity = await _organizationRepository.FirstOrDefaultAsync(u => u.Id == orgId);
            if (organizationEntity is null)
            {
                return Result<LoginResponseDto>.Invalid(new Results.ValidationError
                {
                    ErrorCode = "400",
                    ErrorMessage = "User has no organization"
                });
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (user.GetProperty<UserTypeCategory>("UserTypeCategory") == 0)
                return Result<LoginResponseDto>.Unauthorized();

            using (CurrentTenant.Change(user.TenantId))
            {
                var principal = await _signInManager.CreateUserPrincipalAsync(user);

                bool canCheckout = await _permissionChecker.IsGrantedAsync(principal, CheckoutPermissions.CancelTransaction);
                bool unlinkedEntry = await _permissionChecker.IsGrantedAsync(principal, UnlinkedEntryPermissions.View);

                return Result<LoginResponseDto>.Success(new LoginResponseDto
                {
                    AccessToken = await _authenticationUtility.GenerateTokenAsync(user) ?? string.Empty,
                    Role = roles.FirstOrDefault(),
                    Baseurl = _httpContext?.HttpContext?.Request != null
                        ? $"{_httpContext.HttpContext.Request.Scheme}://{_httpContext.HttpContext.Request.Host}{_httpContext.HttpContext.Request.PathBase}"
                        : string.Empty,
                    TenantId = user.TenantId?.ToString() ?? string.Empty,
                    UnlinkedEntryTransaction = unlinkedEntry,
                    UserId = user.Id.ToString(),
                    OrganizationId = organizationEntity.Id,
                    GracePeriod = organizationEntity.Graceperiod,
                    CanCancelTransaction = canCheckout,
                    OrganizationHasShifts = organizationEntity.Settings?.UseShifts ?? false
                });
            }
        }
    }
}
