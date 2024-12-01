using SmartParking.Entities.Invoices;
using SmartParking.Invoices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Utilities;

public interface IInvoiceUtility
{
    Task<QRCodeDto> GetQrCodeAsync(string? token, int? invoiceId);
    Task<string?> GetTokenAsync();
}
