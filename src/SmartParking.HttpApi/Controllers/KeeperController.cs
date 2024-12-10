using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Authentication;
using SmartParking.Entities.Invoices;
using SmartParking.GarageTransactionEndPoints;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using SmartParking.Mobile.Keepers;
using SmartParking.Payments;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace SmartParking.Controllers;

[Route("api/Keeper")]
[ApiController]

public class KeeperController : AbpController
{
    private readonly IKeeperAppService _keeperAppService;
    private readonly IAuthenticationAppService _authenticationAppService;
	private readonly ICurrentTenant _currentTenant;
	private readonly ICurrentUser _currentUser;
    private readonly IInvoiceAppService _invoiceAppService;
    private readonly IGarageTransactionEndPointsAppService _garageTransactionEndPointsAppService;
    public KeeperController(IKeeperAppService keeperAppService, IAuthenticationAppService authenticationAppService, ICurrentUser currentUser, ICurrentTenant currentTenant, IInvoiceAppService invoiceAppService, IGarageTransactionEndPointsAppService garageTransactionEndPointsAppService)
    {
        _keeperAppService = keeperAppService;
        _authenticationAppService = authenticationAppService;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
        _invoiceAppService = invoiceAppService;
        _garageTransactionEndPointsAppService = garageTransactionEndPointsAppService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]

    public async Task<Result<LoginResponseDto>> Login(LoginRequestDto input)
    {
        var tenant = await _keeperAppService.GetTenantByPhoneNumberDtoAsync(input.PhoneNumber);
        _currentTenant.Change(tenant?.Id ?? null);
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

    [HttpGet("invoice")]
    [Authorize]
    public async Task<Result<List<InvoiceDto>>> GetAllInvoicesByOrganizationId([FromQuery] int organizationId)
    {
        return await _keeperAppService.GetAllInvoicesByOrganizationIdAsync(organizationId);
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
		return await _keeperAppService.GetAllGarageTransactionsWithNoPlateNumberAsync(organizationId,userId);
	}

    [HttpGet("garageTransaction")]
    [Authorize]
    public async Task<Result<List<GarageTransactionDto>>> GetAllGarageTransactions([FromQuery] int organizationId , Guid userId)
    {
        return await _keeperAppService.GetAllGarageTransactionsAsync(organizationId , userId);
    }

    [HttpPut("correctTransaction")]
	[Authorize]
	public async Task<Result<LoginResponseDto>> CorrectTransactionByPlateNumber([FromQuery] int garageTransactionId , string plateNumber)
	{
		return await _keeperAppService.CorrectTransactionByPlateNumberAsync(garageTransactionId, plateNumber);
	}

    [HttpGet("existingTransaction")]
    [Authorize]
    public async Task<Result<List<GarageTransactionDto>>> GetAllExistingTransactions([FromQuery] int organizationId, Guid userId)
    {
        return await _keeperAppService.GetAllExistingTransactionsAsync(organizationId,userId);
    }
    
    [HttpGet("PayAtExitCamera")]
    [Authorize]
    public async Task<Result<PaymentCheckoutDto>> PayInvoice(int InvoiceId)
    {
        return await _keeperAppService.CheckoutAsync(InvoiceId);
    }
    
    [HttpGet("GetUnpaidInvoicesForPlateNumber")]
    [Authorize]
    public async Task<Result<List<InvoiceDto>>> GetUnpaidInvoicesForPlateNumber(int OrganizationId , string PlateNumber)
    {
        return await _invoiceAppService.GetUnpaidInvoiceForKeeperInHisOrganization(OrganizationId, PlateNumber);
    }
    
    [HttpGet("PayInvoiceByCustomerRequest")]
    [Authorize]
    public async Task<Result<InvoiceDto>> PayInvoiceByCustomerRequest(int InvoiceId, string PaymentType)
    {
       return await _keeperAppService.PayCashForCustomerRequestAsync(InvoiceId,  PaymentType);

    }
    [HttpGet("KeeperCheckout")]
    [Authorize]
    public async Task<Result<InvoiceDto>> KeeperCheckoutForTransaction(int transactionId, string PaymentType)
    {
        var invoice=await _garageTransactionEndPointsAppService.CustomerCheckoutFromKeeperMobile(transactionId);
       return await _keeperAppService.PayCashForCustomerRequestAsync(invoice.Value.Id, PaymentType);


    }


    [HttpGet("KeeperPreCheckout")]
    [Authorize]
    public async Task<Result<CreateUpdateInvoiceDto>> KeeperPreCheckoutForTransaction(int transactionId)
    {
        return await _garageTransactionEndPointsAppService.CustomerPreCheckoutFromKeeperMobile(transactionId);

    }

}
