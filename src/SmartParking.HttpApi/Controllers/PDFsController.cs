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

    public PDFsController(IPdfGeneratorFactoryAppService pdfFactoryAppservice)
    {
        _pdfFactoryAppservice = pdfFactoryAppservice;
    }

    [HttpGet("getpdf/{PaymentId}")]
    public async Task<IActionResult> GetInvoicePDF(Guid PaymentId)
    {
        var stream = await _pdfFactoryAppservice.ResolveGeneratorAsync(PaymentId);
        return new FileContentResult(stream.GetAllBytes(), "application/pdf");
    }


}
