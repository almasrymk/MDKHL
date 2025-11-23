using Microsoft.JSInterop;
using Org.BouncyCastle.Asn1.Ocsp;
using SmartParking.BarrierAppService;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Enums;
using SmartParking.Invoices;
using SmartParking.Organizations;
using SmartParking.Payments;
using SmartParking.Utilities;

namespace SmartParking.Blazor.Components.Pages.LastInvoiceAtCamera;

public partial class LastInvoiceAtCamera
{
    private InvoiceDto Invoice = new InvoiceDto();
    private OrganizationDto _organization = new OrganizationDto();
    private bool NoInvoice { get; set; } = false;
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Discount { get; set; } = default;
    public decimal InvoicePrice { get; set; } = default;
    private Task HandleAddDiscount(decimal selected)
    {
        Discount = selected;
        if(Discount==0 || Discount == default)
        {
            Invoice.InvoicePrice = InvoicePrice;
        }
        else
        {

            Invoice.InvoicePrice -= Discount;
        }
        return Task.CompletedTask;
    }

    private void OnSelectPaymentMethod(PaymentMethod selected)
    {
        PaymentMethod = selected;
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _organization = await OrganizationAppService.GetOrganizationForCurrentTenatAsync();

            Invoice = await InvoiceAppService.GetLatestInvoiceForOrganization();
            InvoicePrice = Invoice.InvoicePrice;
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
            var garageTransactionDto=await GarageTransactionAppService.GetAsync(Invoice.GarageTransactionExitId);
            var OpenGateService =await  OpenGateFactoryService.GetOpeningGateService(_organization);  
            var result =await OpenGateService.OpenGateAsync(garageTransactionDto);
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
            PaymentCategory = PaymentCategory.PaymentParking,
            InvoiceId = Invoice.Id,
            PaymentMethod = PaymentMethod,
            Amount = PaymentUtilityCalculationts.GetPriceForPayment(InvoicePrice, Discount),
            CustomerId = Invoice.CustomerId,
            PaymentName = PaymentMethod.Cash.ToString()
        };
        PaymentCheckoutDto? payment = await PaymentAppService.CreateCheckoutInvoicePaymentWebAsync(paymentChekout);
        return payment;
    }
}
