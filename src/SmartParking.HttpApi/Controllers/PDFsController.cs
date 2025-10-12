using Microsoft.AspNetCore.Mvc;
using SmartParking.PDF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;

namespace SmartParking.Controllers;
[Route("api/mobile/PDFs")]
[ApiController]
public class PDFsController : AbpController
{
    private readonly IPdfGeneratorFactoryAppService _pdfFactoryAppservice;
    private const string Thermal = "thermal7cm";
    private const string A4 = "a4";
    public PDFsController(IPdfGeneratorFactoryAppService pdfFactoryAppservice)
    {
        _pdfFactoryAppservice = pdfFactoryAppservice;
    }

    [HttpGet("getthermalpdf/{PaymentId}")]
    public async Task<IActionResult> GetPaymentThermalDF(Guid PaymentId)
    {
        var stream = await _pdfFactoryAppservice.ResolveGeneratorAsync(PaymentId, Thermal);
        return new FileContentResult(stream.GetAllBytes(), "application/pdf");
    }
    [HttpGet("geta4pdf/{PaymentId}")]
    public async Task<IActionResult> GetPaymentA4PDF(Guid PaymentId)
    {
        var stream = await _pdfFactoryAppservice.ResolveGeneratorAsync(PaymentId, A4);
        return new FileContentResult(stream.GetAllBytes(), "application/pdf");
    }


}
