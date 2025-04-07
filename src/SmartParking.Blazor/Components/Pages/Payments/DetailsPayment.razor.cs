using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.Invoices;
using SmartParking.Enums;
using SmartParking.Invoices;
using SmartParking.Payments;
using System;
using System.Threading.Tasks;

namespace SmartParking.Blazor.Components.Pages.Payments;

public partial class DetailsPayment
{
    [Parameter] public string Id { get; set; }  // Capture string ID
    private PaymentGetResponseDto Payment {  get; set; } = new PaymentGetResponseDto();
    private string CreationTime { get; set; }
    private string Amount { get; set; }
    private string LocalizedPaymentCategory{ get; set; }
    private string LocalizedPaymentMethod{ get; set; }
    private string LocalizedPaymentStatus{ get; set; }
    private string LocalizedPaymentCurrency{ get; set; }
    private string LocalizedPaymentInvoiceType{ get; set; }

    private string AmountDiscount { get; set; }


    private async Task FetchPayment()
    {
        Payment = await PaymentWebAppService.GetMdkhlAsync(Guid.Parse(Id));
        CreationTime = Payment.CreationTime.ToString();
        Amount = Payment.Amount.ToString();
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
        };
        return DialogService.ShowAsync<InvoiceDetailsViewDialog>("", parameters);


    }
    private void Back()
    {
        Navigation.NavigateTo("/payments");
    }
}
