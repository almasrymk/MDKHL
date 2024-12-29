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
    private async Task FetchPayment()
    {
        Payment = await PaymentWebAppService.GetMdkhlAsync(Guid.Parse(Id));
        CreationTime = Payment.CreationTime.ToString();
        Amount = Payment.Amount.ToString();
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
            ["CommercialRegisteration"] = Payment.Organization.CommercialRegistrationNumber,
            ["OrganizationAddress"] = Payment.Organization.Address,
        };
        return DialogService.ShowAsync<InvoiceDetailsViewDialog>("", parameters);


    }
    private void Back()
    {
        Navigation.NavigateTo("/payments");
    }
}
