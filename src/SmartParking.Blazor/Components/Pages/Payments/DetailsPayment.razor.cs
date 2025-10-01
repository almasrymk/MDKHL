using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.Invoices;
using SmartParking.Enums;
using SmartParking.Invoices;
using SmartParking.Migrations;
using SmartParking.Payments;
using SmartParking.Utilities;
using System;
using System.Threading.Tasks;

namespace SmartParking.Blazor.Components.Pages.Payments;

public partial class DetailsPayment
{
    [Parameter] public string Id { get; set; }  // Capture string ID
    private PaymentGetResponseDto Payment {  get; set; } = new PaymentGetResponseDto();
    private string CreationTime { get; set; }
    private string Amount { get; set; }
    public string Vat {  get; set; }
    public string Discount {  get; set; }
    public string Total { get; set; }
    public string CashierName { get; set; }
    private string LocalizedPaymentCategory{ get; set; }
    private string LocalizedPaymentMethod{ get; set; }
    private string LocalizedPaymentStatus{ get; set; }
    private string LocalizedPaymentCurrency{ get; set; }
    private string LocalizedPaymentInvoiceType{ get; set; }

    private string AmountDiscount { get; set; }


    private async Task FetchPayment()
    {
        Payment = await paymentWebAppService.GetMdkhlAsync(Guid.Parse(Id));

        CreationTime = Payment?.CreationTime.ToString() ?? "";
        var invoicePrice = Payment?.Invoice?.InvoicePrice ?? 0m;
        var packagePrice = Payment?.Package?.Price ?? 0m;

        if (Payment?.Invoice is not null)
        {
            Amount = invoicePrice.ToString();
        }
        else if (Payment?.Package is not null)
        {
            Amount = packagePrice.ToString();
        }
        else
        {
            Amount = "0";
        }
        var amountDecimal = decimal.Parse(Amount);
        Vat = (amountDecimal * 0.15m).ToString();
        if (Payment?.Invoice is not null)
        {
            Discount = Payment.Invoice.Discount.ToString();
        }
        else if (Payment?.Package is not null)
        {
            Discount = ((amountDecimal - packagePrice) + (amountDecimal * 0.15m)).ToString();
        }
        else
        {
            Discount = "0";
        }
        Total = Payment?.Amount.ToString() ?? "0";


        LocalizedPaymentCategory = Payment?.PaymentCategory != null ? L[Payment.PaymentCategory] : "";
        LocalizedPaymentMethod = Payment?.PaymentMethod != null ? L[Payment.PaymentMethod] : "";
        LocalizedPaymentStatus = Payment?.PaymentStatus != null ? L[Payment.PaymentStatus] : "";
        LocalizedPaymentCurrency = Payment?.Currency != null ? L[Payment.Currency] : "";

        LocalizedPaymentInvoiceType = Payment?.Invoice?.InvoiceType != null ? L[Payment.Invoice.InvoiceType] : "";

        AmountDiscount = Payment?.Invoice != null
            ? Math.Round(((Payment.Invoice.InvoicePrice + (Payment.Invoice.InvoicePrice * 0.15m)) - (Payment.Amount)), 2).ToString()
            : "0";
    }


    protected override async Task OnInitializedAsync()
    {
       await FetchPayment();
        
    }
    private Task ViewInvoiceDetails()
    {

        
        var parameters = new DialogParameters
        {

            ["Invoice"] = Payment.Invoice is not null ? Payment.Invoice:null,
            ["ID"] = Payment.Invoice is not null ? Payment.Invoice.Id : null,
            ["Package"]= Payment.Package is not null ? Payment.Package : null,
            ["CommercialRegisteration"] = Payment.Organization.TaxNumber,
            ["OrganizationAddress"] = Payment.Organization.Address,
            ["Code"] = Payment.PaymentNumber??"",
            ["PaymentPrice"] = Payment.Amount,
            ["CashierName"] = Payment.CashierName
        };
        return DialogService.ShowAsync<InvoiceDetailsViewDialog>("", parameters);


    }
    private void Back()
    {
        Navigation.NavigateTo("/payments");
    }

    private async Task ResendZatca()
    {
        var res = await paymentAppService.ResendZatca(Payment.Id);
        if (res)
            Navigation.NavigateTo("/payments");
        else
        {
            var uri = Navigation.Uri;
            Navigation.NavigateTo(uri, forceLoad: true);
        }
    }
}