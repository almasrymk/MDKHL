using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.PDF;

public record InvoiceLocalizationData(
    string CurrentPath,
    string QRCodePath,
    string InvoiceNumber,
    string TicketId,
    string SimplifiedTaxInvoice,
    string HeaderVatReceiptPayment,
    string Vats,
    string Sar,
    string CashierNo,
    string OrganizationName,
    string OrganizationAddress,
    string PlateNumber,
    string EntranceTime,
    string ExitTime,
    string Subscription,
    string Hour,
    string Duration,
    string Hours,
    string TotalPreVAT,
    string TotalIncludeVAT,
    string Discount,
    string After,
    string InvoiceNumberView,
    decimal Price,
    decimal PaymentPrice
);
