using Microsoft.AspNetCore.Components;
using SmartParking.CustomerPackages.Dtos;
using SmartParking.Entities.Discounts;
using SmartParking.Invoices;
using SmartParking.Packages;
using SmartParking.Payments.PaymentRelations;
using System.Text.Json;

namespace SmartParking.Blazor.Components.Pages.Invoices;

public partial class InvoiceDetailsViewDialog
{
    [Parameter] public InvoiceDto? Invoice { get; set; }
    [Parameter] public CustomerPackageDto? Package { get; set; }
    [Parameter] public PaymentDetailsDto? PaymentDetails { get; set; }
    [Parameter] public string CommercialRegisteration { get; set; }
    [Parameter] public Guid PaymentId { get; set; }
    [Parameter] public string OrganizationAddress { get; set; }
    [Parameter] public string Code { get; set; }
    [Parameter] public decimal PaymentPrice { get; set; }
    [Parameter] public string CashierName { get; set; }
    [Parameter] public string OrganizationArabicName { get; set; }
    [Parameter] public string OrganizationEnglishName { get; set; }
    public string imagepath { get; set; }
    [Parameter] public string OrganizationPicture { get; set; }
    [Parameter] public string QRCode { get; set; }
    [Parameter] public string Amount { get; set; }
    [Parameter] public string Vat { get; set; }
    [Parameter] public string Discount { get; set; }
    [Parameter] public string Total { get; set; }
    [Parameter] public string JsonString { get; set; }
    private Dictionary<string, object> _data;
    public decimal price { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public bool HasDiscount { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        if (!string.IsNullOrEmpty(JsonString))
        {
            try
            {
                // Deserialize the JSON string into a dictionary
                _data = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonString);
            }
            catch (Exception ex)
            {
                _data = new Dictionary<string, object> { { "Error", "Invalid JSON format" } };
            }
        }
        if (Invoice is not null)
        {
            price = Invoice.InvoicePrice;
            title = "Invoice";
            description = Invoice.InvoiceId.ToString();
            var tprice = Math.Round((price + (price * 0.15m)), 2);
            if (tprice != PaymentPrice)
            {
                HasDiscount = true;
            }
        }
        else
        {

            price = Package.PriceOfPackage ?? PaymentDetails.Price;
            title = "Package";
            description = Code;
            if (Package.PriceOfPackage != null)
                HasDiscount = Package.Discount != default;
            HasDiscount = PaymentDetails.Discount != default;
        }
        imagepath = await QRCodeAppService.CreateQRCOdeImageAsync(QRCode);
        StateHasChanged();


    }
}
