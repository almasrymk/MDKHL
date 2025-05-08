using Microsoft.JSInterop;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Enums;
using SmartParking.Invoices;
using SmartParking.Payments;
using SmartParking.Utilities;

namespace SmartParking.Blazor.Components.Pages.LastInvoiceAtCamera;

public partial class LastInvoiceAtCamera
{
    private InvoiceDto Invoice = new InvoiceDto();
    private bool NoInvoice { get; set; } = false;
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Discount { get; set; } = default;
    private void HandleAddDiscount(decimal selected)
    {
        Discount = selected;
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {

            Invoice = await InvoiceAppService.GetLatestInvoiceForOrganization();
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, ex.Message, MudBlazor.Severity.Warning);
            NoInvoice = true;
        }

    }
    private async Task PayInvoice()
    {

        if (Discount != default)
        {
            await InvoiceAppService.UpdateDiscountForInvoice(Invoice.Id, Discount);
        }

        PaymentCheckoutDto payment = await RequestToPaymentCheckout();
        if (payment != null)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.ADD]}", MudBlazor.Severity.Success);
            await Task.Delay(500);
            await OpenGate();
            await JSRuntime.InvokeAsync<object>("open", $"/payments/view/{payment.PaymentId}", "_blank");

        }
        else
        {
            await OpenGate();
            Refresh();

        }
    }
    private async Task OpenGate()
    {
        try
        {
            var result = await InvoiceUtility.SendRequestToOpenGate(Invoice.GarageTransactionExitId);
            if (result == true)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L["OpenGate"]}", MudBlazor.Severity.Success);
                await Task.Delay(1000);
                Navigation.Refresh(true);
            }
            else
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L["OpenGate"]}", MudBlazor.Severity.Error);

            }
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, ex.Message, MudBlazor.Severity.Error);
        }
    }
    private async Task Cancel()
    {
        var ExitTransctionId = Invoice.GarageTransactionExitId;
        await InvoiceAppService.DeleteAsync(Invoice.Id);
        await GarageTransactionAppService.DeleteAsync(ExitTransctionId);
        SnackBar.ShowSnackbar(Snackbar, @L["Done"] + " " + @L["Delete"] + " " + @L["Invoice"], MudBlazor.Severity.Success);
        await Task.Delay(1000);
        Refresh();
    }

    private void Refresh()
    {
        Navigation.Refresh(true);

    }

    private async Task<PaymentCheckoutDto?> RequestToPaymentCheckout()
    {
        var paymentChekout = new PaymentCheckoutRequestDto
        {
            OrganizationId = Invoice.OrganizationId,
            PaymentCategory = Invoice.IsPackage ? PaymentCategory.Package : PaymentCategory.PaymentParking,
            InvoiceId = Invoice.Id,
            PaymentMethod = PaymentMethod,
            Amount = PaymentUtilityCalculationts.GetPriceForPayment(Invoice.InvoicePrice, Discount),
            CustomerId = Invoice.CustomerId,
            PaymentName = PaymentMethod.Cash.ToString()
        };
        PaymentCheckoutDto? payment = await PaymentAppService.CreateCheckoutInvoicePaymentWebAsync(paymentChekout);
        return payment;
    }
}
