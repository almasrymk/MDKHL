using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using SmartParking.Blazor.Components.PosTerminal;
using SmartParking.Blazor.Components.Shared;
using SmartParking.CustomerPackages;
using SmartParking.Enums;
using SmartParking.Enums.PosTerminal;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using SmartParking.Organizations;
using SmartParking.Payments;
using SmartParking.Utilities;

namespace SmartParking.Blazor.Components.Pages.PreCheckout_Invoice;

public partial class PreCheckoutForInvoice
{
    [Inject] private IOrganizationsAppService OrganizationAppService { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;

    [Parameter]
    public int GarageTransactionId { get; set; }

    public CreateUpdateInvoiceDto Invoice { get; set; } = new();
    public GarageTransactionDto GarageTransactionDto { get; set; } = new();
    public int Id { get; set; }
    private bool NoInvoice => Invoice == null || string.IsNullOrEmpty(Invoice?.PlateNumber);
    public decimal Discount { get; set; } = default;
    public decimal InvoicePrice { get; set; } = default;
    private PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    private bool isPaying = false;
    private Task HandleAddDiscount(decimal selected)
    {
        Invoice.Discount = selected;
        Invoice.InvoicePrice = InvoicePrice;

        return Task.CompletedTask;

    }

    private void OnSelectPaymentMethod(PaymentMethod selected) => PaymentMethod = selected;
    private void Cancel()
    {
        Navigation.NavigateTo("/garagetransactions");
    }

    private async Task PayInvoice()
    {
        isPaying = true;
        try
        {
            if (PaymentMethod == PaymentMethod.POS)
            {

                var checkoutRequest = new PaymentCheckoutRequestDto
                {
                    OrganizationId = Invoice.OrganizationId,
                    PaymentCategory = PaymentCategory.PaymentParking,
                    InvoiceId = Id,
                    PaymentMethod = PaymentMethod,
                    Amount = PaymentUtilityCalculationts.GetPriceForPayment(InvoicePrice, Invoice.Discount),
                    CustomerId = Invoice.CustomerId,
                    PaymentName = PaymentMethod.POS.ToString(),
                    PaymentNumber = string.Empty // Generated inside SavePaymentRequest
                };

                Guid createdPaymentId = Guid.Empty;

                var parameters = new DialogParameters<PosPaymentDialog>
                {
                    { x => x.Request, checkoutRequest },
                    { x => x.OnPaymentCreated, EventCallback.Factory.Create<Guid>(this, id => createdPaymentId = id) },
                    { x => x.OnBeforeSendPurchase, EventCallback.Factory.Create(this, async () =>
                        {
                            if (Id == 0)
                            {
                                var invoice = await keeperTransactionsAppService.Checkout(GarageTransactionDto, Invoice);
                                Id = invoice.Value.Id;
                                if (Discount != default)
                                {
                                    await InvoiceAppService.UpdateDiscountForInvoice(Id, Discount);
                                }
                                checkoutRequest.InvoiceId = Id;
                            }
                        })
                    }
                };

                var options = new DialogOptions { CloseButton = false, MaxWidth = MaxWidth.Small, FullWidth = true };
                var dialog = await DialogService.ShowAsync<PosPaymentDialog>("POS Transaction", parameters, options);
                var result = await dialog.Result;

                if (result != null && !result.Canceled && result.Data is PosTerminalStateSnapshot snapshot)
                {
                    if (snapshot.Outcome == SamaResponseOutcome.Approved)
                    {
                        SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.ADD]}", MudBlazor.Severity.Success);
                        await Task.Delay(500);
                        await JSRuntime.InvokeVoidAsync("eval", $"window.open('/payments/view/{createdPaymentId}', '_blank');");
                    }
                    else
                    {
                        Snackbar.Add(snapshot.SamaDescription ?? "Transaction Declined", MudBlazor.Severity.Warning);
                    }
                }
            }
            else
            {
                var invoice = await keeperTransactionsAppService.Checkout(GarageTransactionDto, Invoice);
                Id = invoice.Value.Id;
                if (Discount != default)
                {
                    await InvoiceAppService.UpdateDiscountForInvoice(Id, Discount);
                }

                PaymentCheckoutDto payment = await RequestToPaymentCheckout();
                if (payment != null)
                {
                    SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.ADD]}", MudBlazor.Severity.Success);
                    await Task.Delay(500);
                    await JSRuntime.InvokeVoidAsync("eval", $"window.open('/payments/view/{payment.PaymentId}', '_blank');");
                }
              
            }
        }
        catch (Exception e)
        {
            Snackbar.Add(L[e.Message], MudBlazor.Severity.Error);
            await Task.Delay(1000);
        }
        finally
        {
            isPaying = false;
        }
    }

    private async Task<PaymentCheckoutDto?> RequestToPaymentCheckout()
    {

        var paymentCheckout = new PaymentCheckoutRequestDto
        {
            OrganizationId = Invoice.OrganizationId,
            PaymentCategory = PaymentCategory.PaymentParking,
            InvoiceId = Id,
            PaymentMethod = PaymentMethod,
            Amount = PaymentUtilityCalculationts.GetPriceForPayment(Invoice.InvoicePrice, Discount),
            CustomerId = Invoice.CustomerId
        };

        return await PaymentAppSerivce.CreateCheckoutInvoicePaymentWebAsync(paymentCheckout);
    }


    protected override async Task OnInitializedAsync()
    {
        try
        {

            GarageTransactionDto = await GarageTransactionAppService.GetAsync(GarageTransactionId);
            Invoice = await keeperTransactionsAppService.PreCheckout(GarageTransactionId);
            InvoicePrice = Invoice.InvoicePrice;
        }
        catch(Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, L[$"{ex.Message}"], MudBlazor.Severity.Error);
        }
        

    }
}
