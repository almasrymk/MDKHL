using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.Invoices;
using SmartParking.Enums;
using SmartParking.Invoices;
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
        CreationTime = Payment.CreationTime.ToString();
        Amount = Payment.Invoice is not null ? Payment.Invoice.InvoicePrice.ToString() : "0";
        Vat = Payment.Invoice is not null ? (Payment.Invoice.InvoicePrice *  0.15m).ToString(): "0";
        Discount  = Payment.Invoice is not null ?  Payment.Invoice.Discount.ToString() :"0";
        Total = Payment.Amount.ToString();
        LocalizedPaymentCategory= L[Payment.PaymentCategory];
        LocalizedPaymentMethod=L[Payment.PaymentMethod];
        LocalizedPaymentStatus=L[Payment.PaymentStatus];
        LocalizedPaymentCurrency = L[Payment.Currency];
        LocalizedPaymentInvoiceType = Payment.Invoice is not null ? L[Payment.Invoice.InvoiceType] : "";
        AmountDiscount= Payment.Invoice is not null ? Math.Round((Payment.Invoice.InvoicePrice+ (Payment.Invoice.InvoicePrice*0.15m))- Payment.Amount, 2).ToString() : "0";

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