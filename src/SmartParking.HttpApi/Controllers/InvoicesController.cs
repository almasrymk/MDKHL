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

namespace SmartParking.Controllers;

[Route("api/mobile/Invoice")]
[ApiController]

public class InvoicesController : AbpController
{
    private readonly IInvoicesAppService _invoicesAppService;
    private readonly IPDFGeneratorAppService _pdfAppservice;


    public InvoicesController(IInvoicesAppService invoicesAppService, IPDFGeneratorAppService pdfAppservice)
    {
        _invoicesAppService = invoicesAppService;
        _pdfAppservice = pdfAppservice;
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
}
    
    

