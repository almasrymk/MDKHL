using Abp.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SmartParking.PDF;

public interface IPDFGeneratorAppService : IApplicationService 
{
    Task<byte[]> GenerateInvoicePdfAsync(int invoiceId);
    byte[] GeneratePdfFromHtmlAsync(string htmlContent);
    Task<string> GenerateInvoiceHtmlAsync(int invoiceId);
}
