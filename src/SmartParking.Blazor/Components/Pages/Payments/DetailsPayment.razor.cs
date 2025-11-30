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
using static MudBlazor.CategoryTypes;

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



    private async Task FetchPayment()
    {
        Payment = await paymentWebAppService.GetMdkhlAsync(Guid.Parse(Id));

        CreationTime = Payment?.CreationTime.ToString() ?? "";
        if (Payment.PaymentCategory == PaymentCategory.Package.ToString())
        {

            Total = Payment.Amount == 0 ? "0.0" : Payment.Amount.ToString();
            Amount = Payment.customerPackageDto?.PriceOfPackage.ToString();
            Vat = Payment.customerPackageDto?.Vat.ToString();
            Discount = Payment.customerPackageDto?.Discount.ToString();
        }
        else
        {
            Total = Payment.Amount == 0 ? "0.0" : Payment.Amount.ToString();
            Amount = Payment.Invoice?.InvoicePrice.ToString();
            Vat =
                Payment.Invoice?.InvoicePrice is decimal price &&
                Payment.Invoice?.Discount is decimal discount &&
                (price - discount) != 0
                    ? Math.Round( (price - discount)*0.15m, 2).ToString()
                    : "0.0"; 
            Discount = Payment.Invoice?.Discount.ToString();
        }

        LocalizedPaymentCategory = Payment?.PaymentCategory != null ? L[Payment.PaymentCategory] : "";
        LocalizedPaymentMethod = Payment?.PaymentMethod != null ? L[Payment.PaymentMethod] : "";
        LocalizedPaymentStatus = Payment?.PaymentStatus != null ? L[Payment.PaymentStatus] : "";
        LocalizedPaymentCurrency = Payment?.Currency != null ? L[Payment.Currency] : "";

        LocalizedPaymentInvoiceType = Payment?.Invoice?.InvoiceType != null ? L[Payment.Invoice.InvoiceType] : "";


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
            ["PaymentId"] = Payment.Id,
            ["Package"]= Payment.customerPackageDto is not null ? Payment.customerPackageDto : null,
            ["CommercialRegisteration"] = Payment.Organization.TaxNumber,
            ["OrganizationAddress"] = Payment.Organization.Address,
            ["Code"] = Payment.PaymentNumber??"",
            ["PaymentPrice"] = Payment.Amount,
            ["CashierName"] = Payment.CashierName,
            ["OrganizationArabicName"] =Payment.Organization.ArabicName,
            ["OrganizationEnglishName"] =Payment.Organization.EnglishName,
            ["OrganizationPicture"] =Payment.Organization.OrganizationPicture,
            ["QRCode"] =Payment.QrCodeInvoice,
            ["Total"] = Total,
            ["Amount"] = Amount,
            ["Vat"] = Vat,
            ["Discount"] = Discount,
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