using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Entities.Organizations;
using SmartParking.Invoices;
using SmartParking.Packages;
using SmartParking.Payments;
using SmartParking.Results;
using System;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;

namespace SmartParking.Controllers;

[Route("api/mobile/payment")]
[ApiController]

public class PaymentController : ControllerBase
{
    private readonly IPaymentAppService _paymentAppService;
    private readonly IPaymentMobileHelper _paymentMobileHelper;
    public PaymentController(IPaymentAppService paymentAppService, IPaymentMobileHelper paymentMobileHelper)
    {
        _paymentAppService = paymentAppService;
        _paymentMobileHelper = paymentMobileHelper;
    }
    [HttpPost("CreateCheckoutPayment")]
    [Authorize]
    public async Task<Result<PaymentCheckoutDto>> CreateCheckoutPayment(PaymentCheckoutRequestDto input)
    {
        return await _paymentAppService.CreateCheckoutPaymentAsync(input);
    }



    [HttpGet("GetInvoicePayementDetails/{oragnizationId}/{CustomerId}/{PaymentId}")]
    [Authorize]
    public async Task<Result<InvoiceDto>> GetInvoicePaymentDetails(int oragnizationId, Guid CustomerId, Guid PaymentId)
    {
        return await _paymentAppService.GetInvoicePaymentDetailsAsync(oragnizationId, CustomerId, PaymentId);
    }

    [HttpGet("GetPackagePayementDetails/{PaymentId}")]
    [Authorize]
    public async Task<Result<PaymentPackageDto>> GetPackagePaymentDetails(Guid PaymentId)
    {
        return await _paymentAppService.GetPackagePaymentDetailsAsync(PaymentId);
    }

    [HttpPost("invoice/{invoiceId}/session")]
    [Authorize]
    public async Task<Result<MobilePaymentSessionDto>> InitializePaymentSession(int invoiceId, [FromQuery] SmartParking.Enums.PaymentMethod paymentMethod)
    {
        return await _paymentAppService.InitializeMobilePaymentSessionAsync(invoiceId, paymentMethod);
    }

    [HttpPost("cancel-invoice/{invoiceId}")]
    [Authorize]
    //[IgnoreAntiforgeryToken]
    public async Task<Result> CancelInvoicePayment(int invoiceId, string searchInput)
    {
        //return Result.Success();
        return await _paymentMobileHelper.HandleCancel(invoiceId, searchInput);
    }

}



