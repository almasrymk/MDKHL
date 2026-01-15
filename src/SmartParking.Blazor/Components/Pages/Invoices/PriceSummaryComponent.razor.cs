using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Enums;

namespace SmartParking.Blazor.Components.Pages.Invoices;

public partial class PriceSummaryComponent
{
    [Parameter] public bool IsPaid { get; set; }
    [Parameter] public bool IsPaying { get; set; }

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
    public EventCallback<PaymentMethod> OnSelectPaymentMethod { get; set; }

    [Parameter]
    public bool HasConfirmPermission { get; set; }
    public decimal InvoicePriceAfterDiscount { get; set; }
    public decimal InvoicePriceBeforeDiscount { get; set; }
    public decimal InvoicePriceWithVat { get; set; }
    public decimal Vat { get; set; }
    public decimal Discount { get; set; } = default;
    public bool CanPayInvoice { get; set; } = true;


    protected override void OnParametersSet()
    {
       Vat = Math.Round((InvoicePrice!.Value - Discount) * 0.15m, 2);
       InvoicePriceWithVat = Math.Round((InvoicePrice!.Value - Discount) + Vat, 2);
       InvoicePriceBeforeDiscount = InvoicePrice.Value;
    }

    private async Task OnAddDiscount(decimal discountValue)
    {
        if (discountValue <= InvoicePriceBeforeDiscount && discountValue >= 0)
        {

            InvoicePriceAfterDiscount = InvoicePriceBeforeDiscount - discountValue;
            if (discountValue != default)
            {
                Discount = discountValue;
            }
            else
            {
                Discount = default;

            }
            CanPayInvoice = true;
            Vat =Math.Round(InvoicePriceAfterDiscount*0.15m,2);
            InvoicePriceWithVat = Math.Round(InvoicePriceAfterDiscount + Vat,2);
            await OnValueSelected.InvokeAsync(discountValue);

        }
        else
        {
            Discount = default;
            Vat = Math.Round(InvoicePriceBeforeDiscount * 0.15m, 2);
            InvoicePriceWithVat = Math.Round(InvoicePriceBeforeDiscount + Vat, 2);
        }
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnPaymentMethod(PaymentMethod PM)
    {
        await OnSelectPaymentMethod.InvokeAsync(PM);
    }
}