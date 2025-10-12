using Microsoft.AspNetCore.Components;
using SmartParking.CustomerPackages;
using SmartParking.Invoices;
using SmartParking.Packages;

namespace SmartParking.Blazor.Components.Pages.Invoices;

public partial class InvoiceDetailsViewDialog
{
    [Parameter] public InvoiceDto? Invoice { get; set; }
    [Parameter] public CustomerPackageDto? Package { get; set; }
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
    public decimal price { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public bool HasDiscount { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
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

            price = Package.PriceOfPackage.Value;
            title = "Package";
            description = Code;
            HasDiscount = Package.Discount != default;
        }
        imagepath = await QRCodeAppService.CreateQRCOdeImageAsync(QRCode);
        StateHasChanged();


    }
}
