using Microsoft.AspNetCore.Mvc;
using SmartParking.Payments.PaymentIntegrations;
using System.Threading.Tasks;

namespace SmartParking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentResultController : ControllerBase
{
    private readonly IOppwaIntegrationAppService _paymentService;

    public PaymentResultController(IOppwaIntegrationAppService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet("hyperpay/result")]
    public async Task<IActionResult> Result([FromQuery] string resourcePath)
    {
        var result = await _paymentService.VerifyHyperPayPaymentAsync(resourcePath);

        if (!result.IsSuccess)
            return Redirect("/payment-failed");

        var payment = await _paymentService.HandlePayment(result.PaymentId);
        
        if (!payment.IsSuccess)
            return Redirect("/payment-failed");

        return Redirect($"/custom-invoice/details/{result.PaymentId}");
    }
}
