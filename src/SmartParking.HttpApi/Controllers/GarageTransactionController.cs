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
    private readonly ICustomerMobileTransactionsAppService _customerGarageTransactionsAppService;
    private readonly ICustomerGarageTransactionActionsAppService _customerGarageTransactionsActionsAppService;



    public GarageTransactionController(IGarageTransactionsAppService garageTransactionsAppService, ICustomerMobileTransactionsAppService customerGarageTransactionsAppService, ICustomerGarageTransactionActionsAppService customerGarageTransactionsActionsAppService)
    {
        _garageTransactionsAppService = garageTransactionsAppService;
        _customerGarageTransactionsAppService = customerGarageTransactionsAppService;
        _customerGarageTransactionsActionsAppService = customerGarageTransactionsActionsAppService;
    }

    [HttpGet("carsingarage")]
    [Authorize]
    public async Task<Result<List<GarageTransactionDto>>> GetCustomerTransactionsInOrganization(int OrganizationId)
    {
       return await _garageTransactionsAppService.GetCustomerCarsInOrganization(OrganizationId);

    }

    [HttpGet("CustomerCheckout")]
    [Authorize]
    public async Task<Result<InvoiceDto>> CustomerCheckoutForTransaction(int transactionId)
    {
        return await _customerGarageTransactionsAppService.Checkout(transactionId);

    }
    
    [HttpGet("CustomerPreCheckout")]
    [Authorize]
    public async Task<Result<CreateUpdateInvoiceDto>> CustomerPreCheckoutForTransaction(int transactionId)
    {
      return await _customerGarageTransactionsAppService.PreCheckout(transactionId);

    }

    [HttpGet("VehiclePreCheckout")]
    [Authorize]
    public async Task<Result<CreateUpdateInvoiceDto>> VehiclePreCheckout(string input)
    {
        return await _customerGarageTransactionsActionsAppService.VehiclePreCheckoutAsync(input);
    }


    [HttpGet("VehicleCheckout")]
    [Authorize]
    public async Task<Result<InvoiceDto>> VehicleCheckout(string input)
    {
        return await _customerGarageTransactionsActionsAppService.VehicleCheckoutAsync(input);
    }







}
