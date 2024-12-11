using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using SmartParking.Payments;
using SmartParking.Repositories;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;

namespace SmartParking.Mobile.Keepers;

[RemoteService(isEnabled: false)]
public class KeeperAppService : ApplicationService, IKeeperAppService
{
    private readonly IRepository<Invoice,int> _invoiceRepository;
    private readonly SignInManager<Volo.Abp.Identity.IdentityUser> _signInManager;
    private readonly UserManager<Volo.Abp.Identity.IdentityUser> _userManager;
    private readonly IOptionsMonitor<OpenIddictServerOptions> _openIddictOptions;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IRepository<Volo.Abp.Identity.IdentityUser, Guid> _userRepository;
    private readonly IRepository<GarageTransaction, int> _garageTransactionRepository;
    private readonly IRepository<Organization, int> _organizationRepoistory;
    private readonly IMyUserRepository _myUserRepository;
    private readonly IRepository<Tenant,Guid> _tenantRepository;
    private readonly IPaymentAppService _paymentAppService;

    public KeeperAppService
        (IRepository<Invoice, int> invoiceRepository,
         SignInManager<Volo.Abp.Identity.IdentityUser> signInManager,
         UserManager<Volo.Abp.Identity.IdentityUser> userManager,
         IOptionsMonitor<OpenIddictServerOptions> openIddictOptions,
         IHttpContextAccessor httpContext,
         IRepository<Volo.Abp.Identity.IdentityUser, Guid> userRepository,
         IRepository<GarageTransaction, int> garageTransactionRepository,
         IRepository<Organization, int> organizationRepoistory,
         IMyUserRepository myUserRepository,
         IRepository<Tenant, Guid> tenantRepository,
         IPaymentAppService paymentAppService)
    {
        _invoiceRepository = invoiceRepository;
        _signInManager = signInManager;
        _userManager = userManager;
        _openIddictOptions = openIddictOptions;
        _httpContext = httpContext;
        _userRepository = userRepository;
        _garageTransactionRepository = garageTransactionRepository;
        _organizationRepoistory = organizationRepoistory;
        _myUserRepository = myUserRepository;
        _tenantRepository = tenantRepository;
        _paymentAppService = paymentAppService;
    }

    /// <inheritdoc/>

    public async Task<Result> CorrectTransactionByPlateNumberAsync(int garageTransactionId, string plateNumber)
	{
        GarageTransaction transaction = new GarageTransaction();

        using (DataFilter.Disable<IMultiTenant>())
        {
             transaction = await _garageTransactionRepository.GetAsync(garageTransactionId);

        }

        if (transaction is null) 
        {
            return Result.NotFound("transaction not found");
        }

        transaction.PlateNumber = plateNumber;
        try
        {
            await _garageTransactionRepository.UpdateAsync(transaction);
            return Result.SuccessWithMessage("PlateNumber corrected successfully");

        }
        catch(Exception ex)
        {
            return Result.Error("PlateNumber not corrected" + ex.Message);

        }

    }

	/// <inheritdoc/>
	public async Task<Result<List<GarageTransactionDto>>> GetAllGarageTransactionsWithNoPlateNumberAsync(int organizationId,Guid userId)
	{
        Volo.Abp.Identity.IdentityUser? user;
        List<GarageTransaction> garageTransaction = new List<GarageTransaction>();

        using (DataFilter.Disable<IMultiTenant>())
        {
            user = await _myUserRepository.FindUserByOrganizationAsync(organizationId, userId);

            if (user is null)
            {
                return Result.Invalid(new Results.ValidationError
                {
                    ErrorMessage = "User not found"
                });
            }
            garageTransaction = await _garageTransactionRepository.GetListAsync(t => t.PlateNumber == null);


        }
        var sortedTransaction = garageTransaction.OrderByDescending(t => t.AccurateTime).ToList();

        var mappedTransaction = ObjectMapper.Map<List<GarageTransaction>, List<GarageTransactionDto>>(sortedTransaction);
        
        if(mappedTransaction.Count  < 0 ) 
        {
            return Result<List<GarageTransactionDto>>.NotFound("Transaction not found");
        }

        return Result<List<GarageTransactionDto>>.Success(mappedTransaction);
	}

    /// <inheritdoc/>
    public async Task<Result<List<GarageTransactionDto>>> GetAllGarageTransactionsAsync(int organizationId,Guid userId)
    {
        Volo.Abp.Identity.IdentityUser? user;
        List<GarageTransaction> garageTransaction = new List<GarageTransaction>();

        using (DataFilter.Disable<IMultiTenant>())
        {
            user = await _myUserRepository.FindUserByOrganizationAsync(organizationId, userId);

            if (user is null)
            {
                return Result.Invalid(new Results.ValidationError
                {
                    ErrorMessage = "User not found"
                });
            }

            garageTransaction = await _garageTransactionRepository.GetListAsync();
        }

        var sortedTransaction = garageTransaction.OrderByDescending(t => t.AccurateTime).ToList();

        var mappedTransaction = ObjectMapper.Map<List<GarageTransaction>, List<GarageTransactionDto>>(sortedTransaction);
        
        if (mappedTransaction.Count < 0)
        {
            return Result<List<GarageTransactionDto>>.NotFound("Transaction not found");
        }

        return Result<List<GarageTransactionDto>>.Success(mappedTransaction);
    }
    /// <inheritdoc/>
    public async Task<Result<List<InvoiceDto>>> GetAllInvoicesByOrganizationIdAsync(int organizationId)
    {
        List<Invoice> unPaidInvoices = new List<Invoice>();

        using (DataFilter.Disable<IMultiTenant>())
        {
             unPaidInvoices = await _invoiceRepository.GetListAsync(invo => invo.OrganizationId == organizationId && invo.InvoiceType == InvoiceType.UnPaid );

        }

        var sortedInvoices = unPaidInvoices
            .OrderByDescending(invo => invo.InvoiceTime)
            .ToList();

        var unpaidInvoicesMap = ObjectMapper.Map<List<Invoice>, List<InvoiceDto>>(sortedInvoices);

        if (unpaidInvoicesMap.Count < 0)
        {
            return Result<List<InvoiceDto>>.NotFound("Invoice Not Found");
        }

        return Result<List<InvoiceDto>>.Success(unpaidInvoicesMap);
    }

	/// <inheritdoc/>
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
                UserId = user.Id.ToString(),
                OrganizationId = user.GetProperty<int>("OrganizationId")

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

    public async Task<Result<List<GarageTransactionDto>>> GetAllExistingTransactionsAsync(int organizationId,Guid userId)
    {
        Volo.Abp.Identity.IdentityUser? user;
        List<GarageTransaction> existingTransactions = new List<GarageTransaction>();

        using (DataFilter.Disable<IMultiTenant>())
        {
            user = await _myUserRepository.FindUserByOrganizationAsync(organizationId, userId);

            if (user is null)
            {
                return Result.Invalid(new Results.ValidationError
                {
                    ErrorMessage = "User not found"
                });
            }

            var entranceTransactions = await _garageTransactionRepository.GetListAsync(t => t.IsIn && t.CheckoutMobile==null);

        }

        var sortedTransaction = existingTransactions.OrderByDescending(t => t.AccurateTime).ToList();

        var mappedTransaction = ObjectMapper.Map<List<GarageTransaction>, List<GarageTransactionDto>>(sortedTransaction);
        
        if (mappedTransaction.Count < 0)
        {
            return Result<List<GarageTransactionDto>>.NotFound("Transaction not found");
        }

        return Result<List<GarageTransactionDto>>.Success(mappedTransaction);
    }
    
	public async Task<Tenant> GetTenantByPhoneNumberDtoAsync(string phoneNum)
	{
		var query = await _tenantRepository.GetQueryableAsync();
		query = query.Where(t => t.Name == phoneNum);
		var tenant = await AsyncExecuter.FirstOrDefaultAsync(query);
		return tenant;
	}

    public async Task<Result<PaymentCheckoutDto>> CheckoutAsync(int invoiceId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var invoice = await _invoiceRepository.GetAsync(invoiceId);
            var paymentRequest = new PaymentCheckoutRequestDto
            {
                PaymentCategory=PaymentCategory.Invoice,
                PaymentMethod=PaymentMethod.Cash,
                Amount= Math.Round((invoice.InvoicePrice + (invoice.InvoicePrice * 0.15m)), 2),
                CustomerId=invoice.CustomerId,
                OrganizationId=invoice.OrganizationId,
                InvoiceId=invoiceId,
            };
            return await _paymentAppService.CreateCheckoutInvoicePaymentWebAsync(paymentRequest);
        }
    }
    public async Task<Result<InvoiceDto>> PayCashForCustomerRequestAsync(int invoiceId,string PaymentType)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var invoice = await _invoiceRepository.GetAsync(invoiceId);
            var paymentRequest = new PaymentCheckoutRequestDto
            {
                PaymentCategory=PaymentCategory.Invoice,
                PaymentMethod=PaymentMethod.Cash,
                Amount= Math.Round((invoice.InvoicePrice + (invoice.InvoicePrice * 0.15m)), 2),
                CustomerId=invoice.CustomerId,
                OrganizationId=invoice.OrganizationId,
                InvoiceId=invoiceId,
                PaymentName= PaymentType,
            };
            var result = await _paymentAppService.CreateCheckoutInvoicePaymentWebAsync(paymentRequest);
            return ObjectMapper.Map<Invoice,InvoiceDto>(invoice);
        }
    }

    public async Task<Result<InvoiceDto>> GetLastInvoiceByOrganizationIdAsync(int organizationId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {

            var query = await _invoiceRepository.GetQueryableAsync();
           
            var invoice = await AsyncExecuter.FirstOrDefaultAsync(
                query.Where(invo => invo.OrganizationId == organizationId && invo.InvoiceType == InvoiceType.UnPaid)
                .OrderByDescending(invo => invo.InvoiceTime));
            return ObjectMapper.Map<Invoice, InvoiceDto>(invoice); 

        }
    }

}
