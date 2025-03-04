using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Authentication;
using SmartParking.Discounts;
using SmartParking.Entities.Invoices;
using SmartParking.GarageTransactionEndPoints;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using SmartParking.Mobile.Invoices;
using SmartParking.Mobile.Keepers;
using SmartParking.MobileInvoices;
using SmartParking.Results;
using SmartParking.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using UserDataDto = SmartParking.Mobile.Keepers.UserDataDto;

namespace SmartParking.Controllers;

[Route("api/Keeper")]
[ApiController]

public class KeeperController : AbpController
{
    private readonly IKeeperAppService _keeperAppService;
    private readonly IAuthenticationAppService _authenticationAppService;
    private readonly IUsersAppService _usersAppService;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly IInvoiceAppService _invoiceAppService;
    private readonly IGarageTransactionEndPointsAppService _garageTransactionEndPointsAppService;
    private readonly IGarageTransactionsAppService _garageTransactionsAppService;
    private readonly IDiscountAppService _discountAppService;
    private readonly IInvoiceUtilityAppService _invoiceUtilityAppService;
    public KeeperController(IKeeperAppService keeperAppService, IAuthenticationAppService authenticationAppService, 
        ICurrentUser currentUser, ICurrentTenant currentTenant, IInvoiceAppService invoiceAppService,IDiscountAppService discountAppService,
        IGarageTransactionEndPointsAppService garageTransactionEndPointsAppService, IUsersAppService usersAppService, 
        IInvoiceUtilityAppService invoiceUtilityAppService = null, IGarageTransactionsAppService garageTransactionsAppService = null)
    {
        _keeperAppService = keeperAppService;
        _authenticationAppService = authenticationAppService;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
        _invoiceAppService = invoiceAppService;
        _garageTransactionEndPointsAppService = garageTransactionEndPointsAppService;
        _usersAppService = usersAppService;
        _invoiceUtilityAppService = invoiceUtilityAppService;
        _garageTransactionsAppService = garageTransactionsAppService;
        _discountAppService = discountAppService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]

    public async Task<Result<LoginResponseDto>> Login(LoginRequestDto input)
    {
        try
        {
            
            if (_currentUser.IsAuthenticated)
            {
                return Result<LoginResponseDto>.Invalid(new ValidationError
                {
                    
                    ErrorCode = "400",
                    ErrorMessage = "You are already logged in."
                });
            }
            return await _keeperAppService.LoginAsync(input);
        }
        catch
        {
            return Result<LoginResponseDto>.Invalid(new Results.ValidationError
            {
                ErrorCode = "403",
                ErrorMessage = "This user not found",
            });
        }
    }

    [HttpGet("invoice")]
    [Authorize]
    public async Task<Result<List<InvoiceDto>>> GetAllInvoicesByOrganizationId([FromQuery] int organizationId)
    {
        return await _keeperAppService.GetAllInvoicesByOrganizationIdAsync(organizationId);
    }    
    
    [HttpGet("paid/invoice")]
    [Authorize]
    public async Task<Result<List<InvoiceDto>>> GetAllInvoicesByOrganizationPaidId([FromQuery] int organizationId)
    {
        return await _keeperAppService.GetAllInvoicesByOrganizationIdPaidAsync(organizationId);
    }
    [HttpGet("lastinvoice")]
    [Authorize]
    public async Task<Result<InvoiceDto>> GetLastInvoiceByOrganizationId([FromQuery] int organizationId)
    {
        return await _keeperAppService.GetLastInvoiceByOrganizationIdAsync(organizationId);
    }

    [HttpGet("garageTransactionWithoutPlateNumber")]
    [Authorize]
    public async Task<Result<List<GarageTransactionDto>>> GetAllGarageTransactionsWithNoPlateNumber([FromQuery] int organizationId, Guid userId)
    {
        return await _keeperAppService.GetAllGarageTransactionsWithNoPlateNumberAsync(organizationId, userId);
    }

    [HttpGet("garageTransaction")]
    [Authorize]
    public async Task<Result<List<GarageTransactionDto>>> GetAllGarageTransactions([FromQuery] int organizationId, Guid userId)
    {
        return await _keeperAppService.GetAllGarageTransactionsAsync(organizationId, userId);
    }

    [HttpPut("correctTransaction")]
    [Authorize]
    public async Task<Result<LoginResponseDto>> CorrectTransactionByPlateNumber([FromQuery] int garageTransactionId, string plateNumber)
    {
        return await _keeperAppService.CorrectTransactionByPlateNumberAsync(garageTransactionId, plateNumber);
    }

    [HttpGet("existingTransaction")]
    [Authorize]
    public async Task<Result<List<GarageTransactionDto>>> GetAllExistingTransactions([FromQuery] int organizationId, Guid userId)
    {
        return await _keeperAppService.GetAllExistingTransactionsAsync(organizationId, userId);
    }

    [HttpGet("PayAtExitCamera")]
    [Authorize]
    public async Task<Result<InvoiceMobileDto>> PayInvoice(int InvoiceId,int discountId)
    {
        var invoice = await _keeperAppService.CheckoutAsync(InvoiceId, discountId);
        var openGate=invoice.IsSuccess&& await _invoiceUtilityAppService.SendRequestToOpenGate(invoice.Value.GarageTransactionExitId);
        return invoice;
    }
    [HttpPut("cancelinvoiceandmapping")]
    [Authorize]
    public async Task<Result<bool>> RemoveInvoiceAndUnMappEntryTransaction(int InvoiceId,int ExitTransactionId)
    {
        try
        {
            await _invoiceAppService.DeleteAsync(InvoiceId);
            await _garageTransactionsAppService.DeleteAsync(ExitTransactionId);
            return Result.Success();
        }
        catch (Exception ex) {
            return Result<bool>.Invalid(new ValidationError
            {
                ErrorCode = "404",
                ErrorMessage = ex.Message
            });

        }
    }

    [HttpGet("GetUnpaidInvoicesForPlateNumber")]
    [Authorize]
    public async Task<Result<List<InvoiceDto>>> GetUnpaidInvoicesForPlateNumber(int OrganizationId, string PlateNumber)
    {
        return await _invoiceAppService.GetUnpaidInvoiceForKeeperInHisOrganization(OrganizationId, PlateNumber);
    }

    [HttpGet("PayInvoiceByCustomerRequest")]
    [Authorize]
    public async Task<Result<InvoiceMobileDto>> PayInvoiceByCustomerRequest(int InvoiceId, string PaymentType,int discountId)
    {
        return await _keeperAppService.PayCashForCustomerRequestAsync(InvoiceId, PaymentType, discountId);

    }
    [HttpGet("KeeperCheckout")]
    [Authorize]
    public async Task<Result<InvoiceMobileDto>> KeeperCheckoutForTransaction(int transactionId, string PaymentType,int discountId)
    {
        var invoice = await _garageTransactionEndPointsAppService.CustomerCheckoutFromKeeperMobile(transactionId);
        return await _keeperAppService.PayCashForCustomerRequestAsync(invoice.Value.Id, PaymentType, discountId);
    }


    [HttpGet("KeeperPreCheckout")]
    [Authorize]
    public async Task<Result<CreateUpdateInvoiceDto>> KeeperPreCheckoutForTransaction(int transactionId)
    {
        var invoice = await _garageTransactionEndPointsAppService.CustomerPreCheckoutFromKeeperMobile(transactionId);
        return invoice;
    }

    [HttpPost("changeImage")]
    [Authorize]
    [IgnoreAntiforgeryToken]
    public async Task<Result<string>> ChangeImage([FromBody] string image)
    {
        var user = await _usersAppService.UpdateImageAsync(_currentUser.Id, image);
        return Result.Success(user.Image);
    }

    [HttpGet("getAllDiscount")]
    [Authorize]
    public async Task<Result<List<DiscountDto>>> GetAllDiscount()
    {
        return await _discountAppService.GetListMobileAsync();
    }

}
