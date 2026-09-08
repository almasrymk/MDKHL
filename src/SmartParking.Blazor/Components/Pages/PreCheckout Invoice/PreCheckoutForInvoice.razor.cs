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
    [Inject] private IInvoicePaymentProcessorAppService InvoicePaymentProcessorAppService { get; set; } = default!;

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
            if (Id == 0)
            {
                var invoice = await keeperTransactionsAppService.Checkout(GarageTransactionDto, Invoice);
                Id = invoice.Value.Id;
            }

            var initResult = await InvoicePaymentProcessorAppService.InitiatePaymentAsync(new ProcessInvoicePaymentRequestDto
            {
                InvoiceId = Id,
                PaymentMethod = PaymentMethod,
                Discount = Discount != default ? Discount : Invoice.Discount
            });

            if (initResult.Result == InvoicePaymentResult.AlreadyPaid)
            {
                Snackbar.Add(L["InvoiceIsAlreadyPaid"], MudBlazor.Severity.Info);
                return;
            }

            if (initResult.Result == InvoicePaymentResult.PaymentInProgress)
            {
                Snackbar.Add(L["PaymentInProgress"], MudBlazor.Severity.Warning);
                return;
            }

            if (initResult.Result == InvoicePaymentResult.Failed)
            {
                Snackbar.Add(initResult.Message ?? L["PaymentFailed"], MudBlazor.Severity.Error);
                return;
            }

            if (PaymentMethod == PaymentMethod.POS)
            {
                var organization = await OrganizationAppService.GetOrganizationForCurrentTenatAsync();

                var checkoutRequest = new PaymentCheckoutRequestDto
                {
                    OrganizationId = Invoice.OrganizationId,
                    PaymentCategory = PaymentCategory.PaymentParking,
                    InvoiceId = Id,
                    PaymentMethod = PaymentMethod,
                    Amount = PaymentUtilityCalculationts.GetPriceForPayment(InvoicePrice, Invoice.Discount),
                    CustomerId = Invoice.CustomerId,
                    PaymentName = PaymentMethod.POS.ToString(),
                    PaymentNumber = initResult.PaymentNumber ?? string.Empty
                };

                Guid createdPaymentId = initResult.PaymentId ?? Guid.Empty;

                var parameters = new DialogParameters<PosPaymentDialog>
                {
                    { x => x.Request, checkoutRequest },
                    { x => x.OrgSettings, organization?.OrganizationSettings },
                    { x => x.ExistingPaymentId, createdPaymentId },
                    { x => x.ExistingPaymentNumber, initResult.PaymentNumber },
                    { x => x.OnPaymentCreated, EventCallback.Factory.Create<Guid>(this, id => createdPaymentId = id) }
                };

                var options = new DialogOptions { CloseButton = false, MaxWidth = MaxWidth.Small, FullWidth = true };
                var dialog = await DialogService.ShowAsync<PosPaymentDialog>("POS Transaction", parameters, options);
                var result = await dialog.Result;

                if (result != null && !result.Canceled && result.Data is PosTerminalStateSnapshot snapshot)
                {
                    var completeResult = await InvoicePaymentProcessorAppService.CompletePosPaymentAsync(new CompletePosPaymentRequestDto
                    {
                        InvoiceId = Id,
                        PaymentId = createdPaymentId,
                        SamaCode = snapshot.SamaCode,
                        RawJsonResult = snapshot.Outcome.ToString(),
                        TerminalId = checkoutRequest.PaymentNumber,
                        TransactionStan = null,
                        Rrn = snapshot.Rrn,
                        AuthCode = snapshot.AuthCode,
                        MaskedPan = snapshot.MaskedPan,
                        CardScheme = snapshot.CardScheme
                    });

                    if (completeResult.Result == InvoicePaymentResult.Completed)
                    {
                        SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.ADD]}", MudBlazor.Severity.Success);
                        await Task.Delay(500);
                        await JSRuntime.InvokeVoidAsync("eval", $"window.open('/payments/view/{createdPaymentId}', '_blank');");
                    }
                    else if (completeResult.Result == InvoicePaymentResult.AlreadyPaid)
                    {
                        Snackbar.Add(completeResult.Message ?? L["InvoiceIsAlreadyPaid"], MudBlazor.Severity.Warning);
                    }
                    else
                    {
                        Snackbar.Add(snapshot.SamaDescription ?? completeResult.Message ?? "Transaction Declined", MudBlazor.Severity.Warning);
                    }
                }
            }
            else
            {
                if (initResult.Result == InvoicePaymentResult.Completed && initResult.PaymentId.HasValue)
                {
                    SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.ADD]}", MudBlazor.Severity.Success);
                    await Task.Delay(500);
                    await JSRuntime.InvokeVoidAsync("eval", $"window.open('/payments/view/{initResult.PaymentId.Value}', '_blank');");
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
            if (Id > 0)
            {
                Navigation.NavigateTo($"/invoices/details/{Id}", true);
            }
        }
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
