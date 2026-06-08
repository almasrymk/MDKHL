using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.PDF.Dtos;

public record PackageLocalizationData(
    string CurrentPath,
    string QRCodePath,
    string PackageNumber,
    string TicketId,
    string SimplifiedTaxInvoice,
    string HeaderVatReceiptPayment,
    string Vats,
    string Sar,
    string CashierNo,
    string OrganizationName,
    string OrganizationAddress,
    string PlateNumber,
    string SubscriptionDate,
    string ExpirationDate,
    string Subscription,
    string Duration,
    string Days,
    string TotalPreVAT,
    string TotalIncludeVAT,
    string Discount,
    string After,
    string PackageNumberView,
    decimal Price,
    decimal PaymentPrice ,
    string PaymentNumber
);
