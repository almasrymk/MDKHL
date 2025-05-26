using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Enums;

namespace SmartParking.Blazor.Components.Pages.Invoices;

public partial class PriceSummaryComponent
{
    [Parameter] public bool IsPaid { get; set; }
    [Parameter] public bool NoInvoice { get; set; }
    [Parameter] public bool ShowOpenGate { get; set; }
    [Parameter] public decimal? InvoicePrice { get; set; }
    [Parameter] public EventCallback PayInvoice { get; set; }
    [Parameter] public EventCallback OpenGate { get; set; }
    [Parameter] public EventCallback Cancel { get; set; }
    [Parameter] public PaymentMethod PaymentMethod { get; set; }
    [Parameter] public int ConfirmSize { get; set; }
    [Parameter] public int ComponentSize { get; set; }
    [Parameter]
    public EventCallback<decimal> OnValueSelected { get; set; }
    
    [Parameter]
    public bool HasConfirmPermission { get; set; }

    public decimal InvoicePriceAfterDiscount { get; set; }
    public decimal InvoicePriceWithVat { get; set; }
    public bool OnChooseDiscount { get; set; } = false;
    public bool CanPayInvoice { get; set; } = true;

    protected override void OnParametersSet()
    {
        InvoicePriceWithVat = Math.Round((InvoicePrice.Value + (InvoicePrice.Value * 0.15m)), 2);
        base.OnParametersSet();
    }
    private async Task OnAddDiscount(decimal discountValue)
    {
        if (discountValue <= InvoicePriceWithVat && discountValue > 0)
        {

            InvoicePriceAfterDiscount = InvoicePriceWithVat - discountValue;
            if (discountValue != default)
                OnChooseDiscount = true;
            else
                OnChooseDiscount = false;
            CanPayInvoice = true;
            await OnValueSelected.InvokeAsync(discountValue);
        }
        else
        {
            CanPayInvoice = false;
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.VAlIDATION_ERROR]}: {L["InvalidValidation"]}", Severity.Error);
        }
    }

}
