using Abp.Application.Services;
using SmartParking.Entities.Invoices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SmartParking.PDF;

public interface IPDFGeneratorAppService : IApplicationService 
{
    Task<byte[]> GenerateInvoicePdfAsync(int invoiceId,string commericialRegisteratio,string qrCode);
    byte[] GeneratePdfFromHtmlAsync(string htmlContent, int invoiceId);
    Task<string> GenerateInvoiceHtmlAsync(int invoiceId, string commericialRegisteratio, string qrCode);
}
