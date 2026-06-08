using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.PDF.Dtos;

public abstract class BaseLocalizationCreateDto
{
    // Common assets
    public string OrganizationPicture { get; set; } = string.Empty;
    public string QRCodePath { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal PaymentAmount { get; set; }

    // Common localization keys
    public string SimplifiedTaxInvoiceKey { get; set; } = "SimplifiedTaxInvoice";
    public string HeaderVatReceiptPaymentKey { get; set; } = "HeaderVatReceiptPayment";
    public string VatKey { get; set; } = "VAT";
    public string SarKey { get; set; } = "SAR";
    public string CashierNameKey { get; set; } = "CashierName";
    public string OrganizationNameKey { get; set; } = "OrganizationNameInvoicePopUp";
    public string OrganizationAddressKey { get; set; } = "OrganizationAddressInvoicePopUp";
    public string TotalPreVatKey { get; set; } = "TotalPreVAT";
    public string TotalIncludeVatKey { get; set; } = "TotalIncludeVAT";
    public string DiscountKey { get; set; } = "Discount";
    public string AfterKey { get; set; } = "After";
    public string InvoiceNumberViewKey { get; set; } = "InvoiceNumberView";
}
