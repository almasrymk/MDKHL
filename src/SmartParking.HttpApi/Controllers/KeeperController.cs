using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using SmartParking.Mobile.Keepers;
using SmartParking.Results;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;

namespace SmartParking.Controllers;

[Route("api/Keeper")]
[ApiController]

public class KeeperController : AbpController
{
    private readonly IKeeperAppService _keeperAppService;
    public KeeperController(IKeeperAppService keeperAppService)
    {
        _keeperAppService = keeperAppService;
    }

    [HttpPost("login")]
    public async Task<Result<LoginResponseDto>> Login(LoginRequestDto input)
    {
        return await _keeperAppService.LoginAsync(input);
    }

    [HttpGet("invoice")]
    [Authorize]
    public async Task<Result<List<InvoiceDto>>> GetAllInvoicesByOrganizationId([FromQuery] int organizationId)
    {
        return await _keeperAppService.GetAllInvoicesByOrganizationIdAsync(organizationId);
    }

    [HttpGet("garageTransactionWithNoPlateNumber")]
    [Authorize]
	public async Task<Result<List<GarageTransactionDto>>> GetAllGarageTransactionsWithNoPlateNumber()
	{
		return await _keeperAppService.GetAllGarageTransactionsWithNoPlateNumberAsync();
	}

    [HttpGet("garageTransaction")]
    [Authorize]
    public async Task<Result<List<GarageTransactionDto>>> GetAllGarageTransactions()
    {
        return await _keeperAppService.GetAllGarageTransactionsAsync();
    }

    [HttpPut("correctTransaction")]
	[Authorize]
	public async Task<Result<LoginResponseDto>> CorrectTransactionByPlateNumber([FromQuery] int garageTransactionId , string plateNumber)
	{
		return await _keeperAppService.CorrectTransactionByPlateNumberAsync(garageTransactionId, plateNumber);
	}
}
