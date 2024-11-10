using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Invoices;
using SmartParking.MobileInvoices;
using SmartParking.Payments;
using SmartParking.Results;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace SmartParking.Controllers;

[Route("api/mobile/payment")]
[ApiController]

public class PaymentController : AbpController
{
    private readonly IPaymentAppService _paymentAppService;
    public PaymentController(IPaymentAppService paymentAppService)
    {
        _paymentAppService = paymentAppService;
    }
    [HttpPost("CreateCheckoutPayment")]
    [Authorize]
    public async Task<Result<PaymentDto>> CreateCheckoutPayment(CreateUpdatePaymentDto input)
    {
        return await _paymentAppService.CreateCheckoutPaymentAsync(input);
    }


    [HttpGet("CheckPaymentStatus/{entityId}")]
    //[Authorize]
    public async Task<Result<PaymentDto>> CheckPaymentStatus(string input , string entityId)
    {
        return await _paymentAppService.GetPaymentStatus(input,entityId);
    }


}



