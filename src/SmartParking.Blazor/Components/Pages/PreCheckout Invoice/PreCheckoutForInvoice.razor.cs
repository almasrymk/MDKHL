using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.PosTerminal;
using SmartParking.Organizations;
using SmartParking.Blazor.Components.Shared;
using SmartParking.CustomerPackages;
using SmartParking.Enums;
using SmartParking.Enums.PosTerminal;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
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
    private bool IsPaying { get; set; } = false;
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
        try
        {
            IsPaying = true;
            var invoice = await keeperTransactionsAppService.Checkout(GarageTransactionDto, Invoice);
            Id = invoice.Value.Id;
            if (Discount != default)
            {
                await InvoiceAppService.UpdateDiscountForInvoice(Id, Discount);
            }

            var organization = await OrganizationAppService.GetOrganizationForCurrentTenatAsync();

            if (PaymentMethod == PaymentMethod.POS)
            {
                var checkoutRequest = new PaymentCheckoutRequestDto
                {
                    OrganizationId = Invoice.OrganizationId,
                    PaymentCategory = PaymentCategory.PaymentParking,
                    InvoiceId = Id,
                    PaymentMethod = PaymentMethod,
                    Amount = PaymentUtilityCalculationts.GetPriceForPayment(Invoice.InvoicePrice, Discount),
                    CustomerId = Invoice.CustomerId,
                    PaymentName = PaymentMethod.POS.ToString(),
                    PaymentNumber = string.Empty
                };

                Guid createdPaymentId = Guid.Empty;

                var parameters = new DialogParameters<PosPaymentDialog>
                {
                    { x => x.Request, checkoutRequest },
                    { x => x.OrgSettings, organization.OrganizationSettings },
                    { x => x.OnPaymentCreated, EventCallback.Factory.Create<Guid>(this, id => createdPaymentId = id) }
                };

                var options = new DialogOptions { CloseButton = false, MaxWidth = MaxWidth.Small, FullWidth = true };
                var dialog = await DialogService.ShowAsync<PosPaymentDialog>("POS Transaction", parameters, options);
                var result = await dialog.Result;

                if (result != null && !result.Canceled && result.Data is PosTerminalStateSnapshot snapshot)
                {
                    if (snapshot.Outcome == SamaResponseOutcome.Approved)
                    {
                        SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]}  {L["Checkout"]}", MudBlazor.Severity.Success);
                        await Task.Delay(1000);
                        Navigation.NavigateTo($"/payments/view/{createdPaymentId}");
                    }
                    else
                    {
                        Snackbar.Add(snapshot.SamaDescription ?? "Transaction Declined", MudBlazor.Severity.Warning);
                    }
                }
            }
            else
            {
                PaymentCheckoutDto payment = await RequestToPaymentCheckout();
                if (payment != null)
                {
                    SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]}  {L["Checkout"]}", MudBlazor.Severity.Success);
                    await Task.Delay(1000);
                    Navigation.NavigateTo($"/payments/view/{payment.PaymentId}");
                }
            }
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, L["GracePeriodValidation"], MudBlazor.Severity.Error);
        }
        finally
        {
            IsPaying = false;
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
