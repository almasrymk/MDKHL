using Microsoft.AspNetCore.Components;
using SmartParking.Invoices;
using SmartParking.Packages;

namespace SmartParking.Blazor.Components.Pages.Invoices;

public partial class InvoiceDetailsViewDialog
{
    [Parameter] public InvoiceDto? Invoice { get; set; }
    [Parameter] public PackageDto? Package { get; set; }
    [Parameter] public string CommercialRegisteration { get; set; }
    [Parameter] public int ID { get; set; }
    [Parameter] public string OrganizationAddress { get; set; }
    [Parameter] public string Code { get; set; }
    [Parameter] public decimal PaymentPrice { get; set; }
    public string imagepath { get; set; }
    public string OrganizationPicture { get; set; }
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
            description = Code;
            var tprice = Math.Round((price + (price * 0.15m)), 2);
            if (tprice != PaymentPrice)
            {
                HasDiscount = true;
            }
        }
        else
        {

            price = Package.Price;
            title = "Package";
            description = Code;
        }

        if (currentTenant.Id == null)
        {

            imagepath = await InvoiceAppService.GetInvoiceQRCodeForMDKHL(ID);
        }
        else
        {
            imagepath = await InvoiceAppService.GetInvoiceQRCodeForOrganization(ID);
            OrganizationPicture = await OrganizationService.GetOrganizationPictureByOrganizationTenant(currentTenant.Id);

        }
        StateHasChanged();


    }
}
