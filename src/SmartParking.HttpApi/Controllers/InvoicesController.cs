using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Invoices;
using SmartParking.MobileInvoices;
using SmartParking.PDF;
using SmartParking.Results;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using System.IO;
using SmartParking.Enums;
using SmartParking.Payments;

namespace SmartParking.Controllers;

[Route("api/mobile/Invoice")]
[ApiController]

public class InvoicesController : AbpController
{
    private readonly IInvoicesAppService _invoicesAppService;
    private readonly IPDFGeneratorAppService _pdfAppservice;
    private readonly IPaymentAppService _paymentAppService;


    public InvoicesController(IInvoicesAppService invoicesAppService, IPDFGeneratorAppService pdfAppservice, IPaymentAppService paymentAppService)
    {
        _invoicesAppService = invoicesAppService;
        _pdfAppservice = pdfAppservice;
        _paymentAppService = paymentAppService;
    }
    [HttpPost]
    [Authorize]
    public async Task<Result<InvoiceDto>> CreateInvoice(CreateUpdateInvoiceDto input)
    {
        return await _invoicesAppService.CreateAsync(input);
    }
    [HttpGet]
    [Authorize]
    public async Task<Result<List<InvoiceDto>>> GetAllInvoices()
    {
        return await _invoicesAppService.GetListAsync();
    }  
    
    [HttpGet("getInvoiceDetails")]
    [Authorize]
    public async Task<Result<InvoiceDto>> GetInvoiceDetails(int invoiceId)
    {
        return await _invoicesAppService.GetAsync(invoiceId);
    }
    [HttpGet("getinvoicepdf/{invoiceId}")]
    public async Task<IActionResult> GetInvoicePDF(int invoiceId)
    {
        var stream = await _pdfAppservice.SaveInvoicePdf(invoiceId);
        return new FileContentResult(stream.GetAllBytes(), "application/pdf");
    }   
    
    [HttpPost("customerpayinvoice")]
    [Authorize]
    public async Task<Result<PaymentCheckoutDto>> PayCustomerInvoice(PayCustomerInvoiceDto payCustomerInvoice)
    {
      return await _paymentAppService.PayCustomerInvoiceAsync(payCustomerInvoice);
    }
}
    
    

