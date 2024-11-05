using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using SmartParking.Entities.Invoices;
using SmartParking.Enums;
using SmartParking.Invoices;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Mobile.Keepers;

public class KeeperAppService : ApplicationService, IKeeperAppService
{
    private readonly IRepository<Invoice,int> _invoiceRepository;
    private readonly SignInManager<Volo.Abp.Identity.IdentityUser> _signInManager;
    private readonly UserManager<Volo.Abp.Identity.IdentityUser> _userManager;
    private readonly IOptionsMonitor<OpenIddictServerOptions> _openIddictOptions;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IRepository<Volo.Abp.Identity.IdentityUser, Guid> _userRepository;
    public KeeperAppService
        (IRepository<Invoice, int> invoiceRepository, 
         SignInManager<Volo.Abp.Identity.IdentityUser> signInManager,
         UserManager<Volo.Abp.Identity.IdentityUser> userManager, 
         IOptionsMonitor<OpenIddictServerOptions> openIddictOptions,
         IHttpContextAccessor httpContext,
         IRepository<Volo.Abp.Identity.IdentityUser, Guid> userRepository)
    {
        _invoiceRepository = invoiceRepository;
        _signInManager = signInManager;
        _userManager = userManager;
        _openIddictOptions = openIddictOptions;
        _httpContext = httpContext;
        _userRepository = userRepository;
    }
    [RemoteService(isEnabled:false)]
    public async Task<Result<List<InvoiceDto>>> GetAllInvoicesByOrganizationId(int organizationId)
    {
        var unPaidInvoices = await _invoiceRepository.GetListAsync(invo=>invo.OrganizationId == organizationId && invo.InvoiceType == InvoiceType.UnPaid);
        
        var sortedInvoices = unPaidInvoices
            .OrderByDescending(invo => invo.InvoiceTime)
            .ToList();
        var unpaidInvoicesMap = ObjectMapper.Map<List<Invoice>, List<InvoiceDto>>(sortedInvoices);

        if (unpaidInvoicesMap.Count < 0)
        {
            return Result<List<InvoiceDto>>.NotFound();
        }

        return Result<List<InvoiceDto>>.Success(unpaidInvoicesMap);
    }

    [RemoteService(isEnabled: false)]
    public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto input)
    {
        if (input.PhoneNumber is null)
        {
            return Result.Invalid(new Results.ValidationError
            {
                ErrorMessage = "Phone Number is not valid"
            });
        }
        using (DataFilter.Disable<IMultiTenant>())
        {
            // Step 1: Validate the user's credentials
            var user = await _userRepository.FirstOrDefaultAsync(u => u.PhoneNumber == input.PhoneNumber);

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

            // Sign in the user
            await _signInManager.SignInAsync(user, isPersistent: false);

            // Re-enable tenant context for tenant-specific properties
            var tenantId = user.TenantId.ToString();

            var roles = await _userManager.GetRolesAsync(user);

            return Result<LoginResponseDto>.Success(new LoginResponseDto
            {
                AccessToken = await GenerateTokenAsync(user) ?? string.Empty,
                Role = roles.FirstOrDefault(),
                Baseurl = $"{_httpContext.HttpContext.Request.Scheme}://{_httpContext.HttpContext.Request.Host}{_httpContext.HttpContext.Request.PathBase}",
                TenantId = tenantId,
                UserId = user.Id.ToString()
            });
        }
    }

    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user for whom the token will be generated.</param>
    /// <returns>A string representing the generated JWT token.</returns>
    private async Task<string> GenerateTokenAsync(Volo.Abp.Identity.IdentityUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>()
        {
            new Claim("userId", user.Id.ToString()),
            new Claim("given_name", user.UserName),
            new Claim("email", user.Email),
            new Claim("role", string.Join(",", roles)),
            new Claim("tenantid", user.TenantId?.ToString() ?? ""),
            new Claim("scope", "address email phone roles profile offline_access SmartParking") //replace Qa with yours

        };
        var options = _openIddictOptions.CurrentValue;
        var descriptor = new SecurityTokenDescriptor
        {
            Audience = "SmartParking", // replace with yours,
            EncryptingCredentials = options.DisableAccessTokenEncryption
                ? null
                : options.EncryptionCredentials.First(),
            Expires = null,
            Subject = new ClaimsIdentity(claims, TokenValidationParameters.DefaultAuthenticationType),
            IssuedAt = DateTime.UtcNow,
            Issuer = $"{_httpContext.HttpContext.Request.Scheme}://{_httpContext.HttpContext.Request.Host}{_httpContext.HttpContext.Request.PathBase}", // replace with yours,
            SigningCredentials = options.SigningCredentials.First(),
            TokenType = OpenIddictConstants.JsonWebTokenTypes.AccessToken,
        };

        var accessToken = options.JsonWebTokenHandler.CreateToken(descriptor);

        return accessToken;
    }


}
