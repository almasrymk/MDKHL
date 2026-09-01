using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Entities.Organizations;
using SmartParking.Invoices;
using SmartParking.Packages;
using SmartParking.Payments;
using SmartParking.Payments.Reconciliation;
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
    private readonly IPaymentWebAppService _paymentWebAppService;
    private readonly IPaymentMobileHelper _paymentMobileHelper;
    private readonly IPaymentReconciliationAppService _paymentReconciliationAppService;

    public PaymentController(
        IPaymentAppService paymentAppService, 
        IPaymentWebAppService paymentWebAppService, 
        IPaymentMobileHelper paymentMobileHelper,
        IPaymentReconciliationAppService paymentReconciliationAppService)
    {
        _paymentAppService = paymentAppService;
        _paymentWebAppService = paymentWebAppService;
        _paymentMobileHelper = paymentMobileHelper;
        _paymentReconciliationAppService = paymentReconciliationAppService;
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
    [HttpPost("package/{packageId}/session")]
    [Authorize]
    public async Task<Result<MobilePaymentSessionDto>> InitializePaymentSessionForPackage(int packageId, [FromQuery] SmartParking.Enums.PaymentMethod paymentMethod)
    {
        return await _paymentAppService.InitializePackagePaymentSessionAsync(packageId, paymentMethod);
    }

    [HttpPost("cancel-invoice/{invoiceId}")]
    [Authorize]
    //[IgnoreAntiforgeryToken]
    public async Task<Result> CancelInvoicePayment(int invoiceId)
    {
        //return Result.Success();
        return await _paymentMobileHelper.HandleCancel(invoiceId);
    }

    [HttpPost("cancel/{paymentId}")]
    [Authorize]
    public async Task<Result> CancelPayment(Guid paymentId)
    {
        try
        {
            await _paymentReconciliationAppService.ProcessNonSuccessPaymentAsync(paymentId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }

    [HttpGet("getinvoicepaymentdetails/{PaymentId}")]
    [Authorize]
    public async Task<Result<PaymentDetailDto>> GetPaymentDetaild(Guid PaymentId)
    {
        return await _paymentWebAppService.GetPaymentDetailAsync(PaymentId);
    }

}



