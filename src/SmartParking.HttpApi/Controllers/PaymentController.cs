using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Entities.Organizations;
using SmartParking.Invoices;
using SmartParking.Packages;
using SmartParking.Payments;
using SmartParking.Results;
using System;
using System.Threading.Tasks;

namespace SmartParking.Controllers;

[Route("api/mobile/payment")]
[ApiController]

public class PaymentController : ControllerBase
{
    private readonly IPaymentAppService _paymentAppService;
    public PaymentController(IPaymentAppService paymentAppService)
    {
        _paymentAppService = paymentAppService;
    }
    [HttpPost("CreateCheckoutPayment")]
    [Authorize]
    public async Task<Result<PaymentCheckoutDto>> CreateCheckoutPayment(PaymentCheckoutRequestDto input)
    {
        return await _paymentAppService.CreateCheckoutPaymentAsync(input);
    }


    [HttpGet("CheckPaymentStatus/{entityId}")]
    //[Authorize]
    [IgnoreAntiforgeryToken]
    public async Task<Result<PaymentDto>> CheckPaymentStatus(string input, string entityId)
    {
        return await _paymentAppService.GetPaymentStatus(input, entityId);
    }

    [HttpGet("GetInvoicePayementDetails/{oragnizationId}/{CustomerId}/{PaymentId}")]
    [Authorize]
    public async Task<Result<InvoiceDto>> GetInvoicePaymentDetails(int oragnizationId, Guid CustomerId, Guid PaymentId)
    {
        return await _paymentAppService.GetInvoicePaymentDetailsAsync(oragnizationId, CustomerId, PaymentId);
    }

    [HttpGet("GetPackagePayementDetails/{oragnizationId}/{CustomerId}/{PaymentId}")]
    [Authorize]
    public async Task<Result<PackageDto>> GetPackagePaymentDetails(int oragnizationId, Guid CustomerId, Guid PaymentId)
    {
        return await _paymentAppService.GetPackagePaymentDetailsAsync(oragnizationId, CustomerId, PaymentId);
    }

}



