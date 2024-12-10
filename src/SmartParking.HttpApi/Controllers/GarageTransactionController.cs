using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Entities.Organizations;
using SmartParking.GarageTransactionEndPoints;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
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
    public async Task<Result<List<GarageTransactionDto>>> GetCustomerTransactionsInOrganization(int OrganizationId)
    {
       return await _garageTransactionsAppService.GetCustomerTransactionsInOrganization(OrganizationId);

    }

    [HttpGet("CustomerCheckout")]
    [Authorize]
    public async Task<Result<InvoiceDto>> CustomerCheckoutForTransaction(int transactionId)
    {
        return await _garageTransactionEndPointsAppService.CustomerCheckoutFromHisMobile(transactionId);

    }
    
    [HttpGet("CustomerPreCheckout")]
    [Authorize]
    public async Task<Result<CreateUpdateInvoiceDto>> CustomerPreCheckoutForTransaction(int transactionId)
    {
      return await _garageTransactionEndPointsAppService.CustomerPreCheckoutFromHisMobile(transactionId);

    }







}
