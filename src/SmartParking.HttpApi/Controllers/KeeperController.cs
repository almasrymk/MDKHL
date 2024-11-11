using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Authentication;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using SmartParking.Mobile.Keepers;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;

namespace SmartParking.Controllers;

[Route("api/Keeper")]
[ApiController]

public class KeeperController : AbpController
{
    private readonly IKeeperAppService _keeperAppService;
    private readonly IAuthenticationAppService _authenticationAppService;
    public KeeperController(IKeeperAppService keeperAppService, IAuthenticationAppService authenticationAppService)
    {
        _keeperAppService = keeperAppService;
        _authenticationAppService = authenticationAppService;
    }

    [HttpPost("login")]
    public async Task<Result<LoginResponseDto>> Login(LoginRequestDto input)
    {
        var tenant = await _authenticationAppService.GetTenantByPhoneNumberDtoAsync(input.PhoneNumber);
        CurrentTenant.Change(tenant?.Id ?? null);
        if (CurrentUser.IsAuthenticated)
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

    [HttpGet("garageTransactionWithoutPlateNumber")]
    [Authorize]
	public async Task<Result<List<GarageTransactionDto>>> GetAllGarageTransactionsWithNoPlateNumber([FromQuery] int organizationId)
	{
		return await _keeperAppService.GetAllGarageTransactionsWithNoPlateNumberAsync(organizationId);
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
    public async Task<Result<List<GarageTransactionDto>>> GetAllExistingTransactions([FromQuery] int organizationId)
    {
        return await _keeperAppService.GetAllExistingTransactionsAsync(organizationId);
    }
}
