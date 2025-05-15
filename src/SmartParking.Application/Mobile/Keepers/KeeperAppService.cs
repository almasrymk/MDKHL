using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using SmartParking.Discounts;
using SmartParking.Entities.Discounts;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using SmartParking.Mobile.Invoices;
using SmartParking.Mobile.Vehicles;
using SmartParking.Payments;
using SmartParking.Repositories;
using SmartParking.Results;
using SmartParking.Utilities;
using SmartParking.Validations;
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
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

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
    private readonly IVehicleAppService _vehicleAppService;
    private readonly IDiscountAppService _discountAppService;


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
         IAuthenticationUtility authenticationUtility,
         IVehicleAppService vehicleAppService,
         IDiscountAppService discountAppService)
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
        _vehicleAppService = vehicleAppService;
        _discountAppService = discountAppService;
    }

    #region Public Methods
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
        var listInvoice=await AsyncExecuter.ToListAsync(_invoiceRepository.GetQueryableAsync().Result.Where(inv => inv.OrganizationId == organizationId).OrderByDescending(inv=>inv.InvoiceTime).Take(100));
        return ObjectMapper.Map<List<Invoice>, List<InvoiceDto>>(listInvoice);
    }
    /// <inheritdoc/>

    public async Task<Result<List<InvoiceDto>>> GetAllInvoicesByOrganizationIdPaidAsync(int organizationId)
    {
        var listInvoice=await AsyncExecuter.ToListAsync(_invoiceRepository.GetQueryableAsync().Result.Where(inv => inv.OrganizationId == organizationId && inv.InvoiceType == InvoiceType.Paid).OrderByDescending(inv=>inv.InvoiceTime).Take(100));
        return ObjectMapper.Map<List<Invoice>, List<InvoiceDto>>(listInvoice);
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
            var organizationEntity=await _organizationRepoistory.FirstOrDefaultAsync(u => u.Id == orgId);
            if (organizationEntity is null) {
                return Result<LoginResponseDto>.Invalid(new Results.ValidationError
                {
                    ErrorCode = "400",
                    ErrorMessage = "User has no organization"
                });
            }
            var roles = await _userManager.GetRolesAsync(user);
           
            if (user.GetProperty<UserTypeCategory>("UserTypeCategory") == 0)
                return Result<LoginResponseDto>.Unauthorized();
            CurrentTenant.Change(user?.TenantId ?? null);
           
            await _signInManager.SignInAsync(user, isPersistent: false);
            var tenantId = user.TenantId.ToString();

            

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
    /// <inheritdoc/>

    public async Task<Result<List<GarageTransactionDto>>> GetAllExistingTransactionsAsync(int organizationId,Guid userId)
    {
        var result = await CheckUserIsFound(organizationId, userId);
        if(result is not null)
        {
            return result;
        }
        var entranceTransactionsQuery = await _garageTransactionRepository.GetQueryableAsync();
        entranceTransactionsQuery = entranceTransactionsQuery.Where(t => t.IsIn).OrderByDescending(t => t.AccurateTime);
        var sortedTransaction = entranceTransactionsQuery.ToList();
        var mappedTransaction = ObjectMapper.Map<List<GarageTransaction>, List<GarageTransactionDto>>(sortedTransaction);
        return mappedTransaction.Count == 0? Result<List<GarageTransactionDto>>.NotFound("Transaction not found") : Result<List<GarageTransactionDto>>.Success(mappedTransaction);
    }

   
    /// <inheritdoc/>

    public async Task<Tenant> GetTenantByPhoneNumberDtoAsync(string phoneNum)
	{
		var query = await _tenantRepository.GetQueryableAsync();
		query = query.Where(t => t.Name == phoneNum);
		var tenant = await AsyncExecuter.FirstOrDefaultAsync(query);
		return tenant;
	}
    /// <inheritdoc/>

    public async Task<Result<InvoiceMobileDto>> CheckoutAsync(int invoiceId , decimal discountValue =default)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var invoice = await _invoiceAppService.GetAsync(invoiceId);
            if (CheckIfInvoiceIsPaid(invoice))
            {
                return Result.Error("InvoiceIsAlreadyPaid");

            }
            if(discountValue != default)
            {
                try
                {
                    await _invoiceAppService.UpdateDiscountForInvoice(invoiceId, discountValue);
                }
                catch(Exception ex)
                {
                    return Result.Error(ex.Message);
                }
                
            }
            var result = await RequestToPaymentCheckout(invoice, discountValue);
            var image = await _invoiceAppService.GetImage(invoice,result is null ? " " : result.QrCodeInvoice);
            return await ProcessingInvoiceMobileDto(discountValue,invoiceId ,image);


        }
    }

   
    /// <inheritdoc/>

    public async Task<Result<InvoiceMobileDto>> PayCashForCustomerRequestAsync(int invoiceId,string PaymentType, decimal discountValue = default)
    {
       return await CheckoutAsync(invoiceId, discountValue);
    }

  
    /// <inheritdoc/>

    public async Task<Result<InvoiceDto>> GetLastInvoiceByOrganizationIdAsync(int organizationId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {

            var query = await _invoiceRepository.GetQueryableAsync();
           
            var invoice = await AsyncExecuter.FirstOrDefaultAsync(
                query.Where(invo => invo.OrganizationId == organizationId && invo.InvoiceType == InvoiceType.UnPaid&&invo.InvoicePrice>0)
                .OrderByDescending(invo => invo.InvoiceTime));
            return ObjectMapper.Map<Invoice, InvoiceDto>(invoice); 

        }
    }

    #endregion
    #region Private Methods
    /// <summary>
    /// Checks whether an invoice is marked as paid.
    /// </summary>
    /// <param name="invoice">The invoice to check.</param>
    /// <returns><c>true</c> if the invoice is marked as paid; otherwise, <c>false</c>.</returns>
    private static bool CheckIfInvoiceIsPaid(InvoiceDto invoice)
    {
        if (invoice.InvoiceType == InvoiceType.Paid.ToString())
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Processes the invoice data to create an <see cref="InvoiceMobileDto"/> with additional details like QR code, vehicle info, and price calculations.
    /// </summary>
    /// <param name="discountValue">The discount value to apply to the invoice.</param>
    /// <param name="invoiceId">The unique identifier of the invoice to process.</param>
    /// <param name="image">The result containing the QR code image for the invoice.</param>
    /// <returns>A task representing the asynchronous operation, containing a <see cref="Result{InvoiceMobileDto}"/> object with the processed invoice details.</returns>
    private async Task<Result<InvoiceMobileDto>> ProcessingInvoiceMobileDto(decimal discountValue, int invoiceId, Result<string> image)
    {
        var invoice = await _invoiceAppService.GetAsync(invoiceId);
        var invoiceDto = ObjectMapper.Map<InvoiceDto, InvoiceMobileDto>(invoice);
        invoiceDto.QrCodeInvoice = image.Value;
        invoiceDto.Vehicle = await _vehicleAppService.GetVehicleByPlateNumberAsync(invoice.PlateNumber);
        invoiceDto.TotalPriceWithVatAndDiscount = PaymentUtilityCalculationts.GetPriceForPayment(invoice.InvoicePrice, discountValue);
        invoiceDto.ExitTransactionImage = invoiceDto.EntryTransactionImage;
        return invoiceDto;
    }

    /// <summary>
    /// Requests a payment checkout by creating a payment request with necessary details like payment category, method, and amount.
    /// </summary>
    /// <param name="invoice">The invoice for which the payment checkout is requested.</param>
    /// <param name="discountValue">The discount value to apply to the payment.</param>
    /// <returns>A task representing the asynchronous operation, containing the <see cref="PaymentCheckoutDto"/> for the payment request.</returns>
    private async Task<PaymentCheckoutDto?> RequestToPaymentCheckout(InvoiceDto invoice, decimal discountValue)
    {
        var paymentChekout = new PaymentCheckoutRequestDto
        {
            PaymentCategory = PaymentCategory.PaymentParking,
            PaymentMethod = PaymentMethod.Cash,
            Amount = PaymentUtilityCalculationts.GetPriceForPayment(invoice.InvoicePrice, discountValue),
            CustomerId = invoice.CustomerId,
            OrganizationId = invoice.OrganizationId,
            InvoiceId = invoice.Id,
            PaymentName = PaymentMethod.Cash.ToString()
        };
        var payment = await _paymentAppService.CreateCheckoutInvoicePaymentWebAsync(paymentChekout);
        return payment;
    }

    /// <summary>
    /// Checks if a user exists within a specific organization by their user ID and organization ID.
    /// </summary>
    /// <param name="organizationId">The organization ID to check within.</param>
    /// <param name="userId">The unique identifier of the user to check.</param>
    /// <returns>A task representing the asynchronous operation, returning <c>null</c> if the user exists, or an error result if the user is not found.</returns>
    private async Task<Result?> CheckUserIsFound(int organizationId, Guid userId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var user = await _myUserRepository.FindUserByOrganizationAsync(organizationId, userId);

            if (user is null)
            {
                return Result.Invalid(new Results.ValidationError
                {
                    ErrorMessage = "User not found"
                });
            }
        }
        return null;
    }
    #endregion
}
