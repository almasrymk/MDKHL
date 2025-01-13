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
using SmartParking.Utilities;
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
    private readonly IInvoiceAppService _invoiceAppService;
    private readonly IAuthenticationUtility _authenticationUtility;

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
         IPaymentAppService paymentAppService,
         IInvoiceAppService invoiceAppService,
         IAuthenticationUtility authenticationUtility)
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
        _invoiceAppService = invoiceAppService;
        this._authenticationUtility = authenticationUtility;
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
        var listInvoice=await AsyncExecuter.ToListAsync(_invoiceRepository.GetQueryableAsync().Result.Where(inv => inv.OrganizationId == organizationId && inv.InvoiceType == InvoiceType.UnPaid).OrderByDescending(inv=>inv.InvoiceTime).Take(100));
        return ObjectMapper.Map<List<Invoice>, List<InvoiceDto>>(listInvoice);
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
            var orgId = user.GetProperty<int>("OrganizationId");
            var organizationEntity=await _organizationRepoistory.FirstOrDefaultAsync(u => u.Id == orgId);
            if (organizationEntity is null) {
                return Result<LoginResponseDto>.Invalid(new Results.ValidationError
                {
                    ErrorCode = "400",
                    ErrorMessage = "User has no organization"
                });
            }
            // Sign in the user
            await _signInManager.SignInAsync(user, isPersistent: false);

            // Re-enable tenant context for tenant-specific properties
            var tenantId = user.TenantId.ToString();

            var roles = await _userManager.GetRolesAsync(user);

            return Result<LoginResponseDto>.Success(new LoginResponseDto
            {
                AccessToken = await _authenticationUtility.GenerateTokenAsync(user) ?? string.Empty,
                Role = roles.FirstOrDefault(),
                Baseurl = $"{_httpContext.HttpContext.Request.Scheme}://{_httpContext.HttpContext.Request.Host}{_httpContext.HttpContext.Request.PathBase}",
                TenantId = tenantId,
                UserId = user.Id.ToString(),
                OrganizationId = organizationEntity.Id,
                GracePeriod = organizationEntity.Graceperiod

            });
        }
    }

    public async Task<Result<List<GarageTransactionDto>>> GetAllExistingTransactionsAsync(int organizationId,Guid userId)
    {
        Volo.Abp.Identity.IdentityUser? user;
        List<GarageTransaction> existingTransactions = new List<GarageTransaction>();
        List<GarageTransaction> entranceTransactions = new List<GarageTransaction>();


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

             entranceTransactions = await _garageTransactionRepository.GetListAsync(t => t.IsIn);

        }

        var sortedTransaction = entranceTransactions.OrderByDescending(t => t.AccurateTime).ToList();

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

    public async Task<Result<InvoiceDto>> CheckoutAsync(int invoiceId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var invoice = await _invoiceRepository.GetAsync(invoiceId);
            if (CheckIfInvoiceIsPaid(invoice))
            {
                return Result.Error("InvoiceIsAlreadyPaid");

            }

            var paymentRequest = new PaymentCheckoutRequestDto
            {
                PaymentCategory=PaymentCategory.PaymentParking,
                PaymentMethod=PaymentMethod.Cash,
                Amount= Math.Round((invoice.InvoicePrice + (invoice.InvoicePrice * 0.15m)), 2),
                CustomerId=invoice.CustomerId,
                OrganizationId=invoice.OrganizationId,
                InvoiceId=invoiceId,
            };

            var result = await _paymentAppService.CreateCheckoutInvoicePaymentWebAsync(paymentRequest);
            var image = await _invoiceAppService.GetInvoiceQRCodeForMDKHL(invoiceId);
            var invoiceDto = ObjectMapper.Map<Invoice, InvoiceDto>(invoice);
            invoiceDto.QrCodeInvoice = image.Value;
            invoiceDto.ExitTransactionImage = invoiceDto.EntryTransactionImage;
            return invoiceDto;
        }
    }
    public async Task<Result<InvoiceDto>> PayCashForCustomerRequestAsync(int invoiceId,string PaymentType)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var invoice = await _invoiceRepository.GetAsync(invoiceId);

            var paymentRequest = new PaymentCheckoutRequestDto
            {
                PaymentCategory = PaymentCategory.PaymentParking,
                PaymentMethod = PaymentMethod.Cash,
                Amount = Math.Round((invoice.InvoicePrice + (invoice.InvoicePrice * 0.15m)), 2),
                CustomerId = invoice.CustomerId,
                OrganizationId = invoice.OrganizationId,
                InvoiceId = invoiceId,
                PaymentName = PaymentType,
                
            };
            var result = await _paymentAppService.CreateCheckoutInvoicePaymentWebAsync(paymentRequest);
            var image =await _invoiceAppService.GetImage(invoice,result.QrCodeInvoice);
            var invoiceDto = ObjectMapper.Map<Invoice, InvoiceDto>(invoice);
            invoiceDto.QrCodeInvoice = image.Value;
            return invoiceDto;
        }
    }

    private static bool CheckIfInvoiceIsPaid(Invoice invoice)
    {
        if (invoice.InvoiceType == InvoiceType.Paid)
        {
            return true;
        }
        return false;
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
