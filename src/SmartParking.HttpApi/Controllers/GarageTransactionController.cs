using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Entities.Organizations;
using SmartParking.GarageTransactionEndPoints;
using SmartParking.GarageTransactions;
using SmartParking.Results;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Controllers;

[Route("api/mobile/[controller]")]
[ApiController]
public class GarageTransactionController : ControllerBase
{
    private readonly IGarageTransactionsAppService _garageTransactionsAppService;
    private readonly IGarageTransactionEndPointsAppService _garageTransactionEndPointsAppService;

    public GarageTransactionController(IGarageTransactionsAppService garageTransactionsAppService, IGarageTransactionEndPointsAppService garageTransactionEndPointsAppService)
    {
        _garageTransactionsAppService = garageTransactionsAppService;
        _garageTransactionEndPointsAppService = garageTransactionEndPointsAppService;
    }

    [HttpGet("carsingarage")]
    [Authorize]
    public async Task<Result<List<string>>> GetCustomerPlateNumbersInGarage(Guid CustomerId , int GarageId)
    {
       return await _garageTransactionsAppService.GetCustomerPlateNumbersInGarage(CustomerId,GarageId);

    }

    [HttpGet("platenumbercheckout")]
    [Authorize]
    public async Task<Result<List<string>>> CheckoutForPlateNumber(string plateNumber)
    {
       return await _garageTransactionEndPointsAppService.

    }






}
