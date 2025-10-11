using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.PDF;

public class InvoiceLocalizationCreateDto
{
    public string OrganizationPicture { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public decimal InvoicePrice { get; set; }
    public decimal PaymentAmount { get; set; }
    public string InvoiceKey { get; set; } = "Invoice";
    public string TicketIdKey { get; set; } = "TicketId";
    public string SimplifiedTaxInvoiceKey { get; set; } = "SimplifiedTaxInvoice";
    public string HeaderVatReceiptPaymentKey { get; set; } = "HeaderVatReceiptPayment";
    public string VatKey { get; set; } = "VAT";
    public string SarKey { get; set; } = "SAR";
    public string CashierNameKey { get; set; } = "CashierName";
    public string OrganizationNameKey { get; set; } = "OrganizationNameInvoicePopUp";
    public string OrganizationAddressKey { get; set; } = "OrganizationAddressInvoicePopUp";
    public string PlateNumberKey { get; set; } = "PlateNumber";
    public string EntranceTimeKey { get; set; } = "EntranceTime";
    public string ExitTimeKey { get; set; } = "ExitTime";
    public string SubscriptionKey { get; set; } = "Subscription";
    public string HourKey { get; set; } = "Hour";
    public string DurationKey { get; set; } = "TotalHours";
    public string HoursKey { get; set; } = "Hours";
    public string TotalPreVatKey { get; set; } = "TotalPreVAT";
    public string TotalIncludeVatKey { get; set; } = "TotalIncludeVAT";
    public string DiscountKey { get; set; } = "Discount";
    public string AfterKey { get; set; } = "After";
    public string InvoiceNumberViewKey { get; set; } = "InvoiceNumberView";
}
