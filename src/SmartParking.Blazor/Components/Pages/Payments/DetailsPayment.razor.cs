using Microsoft.AspNetCore.Components;
using SmartParking.Blazor.Components.DataGrid;
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
        Guid id = Guid.Parse(Id);
        Payment = await PaymentWebAppService.GetMdkhlAsync(id);
        CreationTime = Payment.CreationTime.ToString();
        Amount = Payment.Amount.ToString();
    }
    protected override async Task OnInitializedAsync()
    {
       await FetchPayment();
        await base.OnInitializedAsync();
        
    }
    private void Back()
    {
        Navigation.NavigateTo("/payments");
    }
}
