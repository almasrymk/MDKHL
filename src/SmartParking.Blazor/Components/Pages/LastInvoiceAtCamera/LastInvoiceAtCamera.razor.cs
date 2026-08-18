using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.PosTerminal;
using SmartParking.Enums;
using SmartParking.Enums.PosTerminal;
using SmartParking.Invoices;
using SmartParking.OpeningGate;
using SmartParking.Organizations;
using SmartParking.Payments;
using SmartParking.Shifts;
using SmartParking.Utilities;

namespace SmartParking.Blazor.Components.Pages.LastInvoiceAtCamera;

public partial class LastInvoiceAtCamera
{
    [Inject] private IDialogService DialogService { get; set; } = default!;

    private InvoiceDto Invoice = new InvoiceDto();
    private OrganizationDto _organization = new OrganizationDto();
    private ICollection<ShiftWatcherActionDto> _filteredWatchersByShiftZone = new List<ShiftWatcherActionDto>();
    private bool NoInvoice { get; set; } = false;
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Discount { get; set; } = default;
    public decimal InvoicePrice { get; set; } = default;
    private bool isPaying = false;


    private string? _selectedDeviceId;
    private string? SelectedDeviceId
    {
        get => _selectedDeviceId;
        set
        {
            if (_selectedDeviceId == value) return;
            _selectedDeviceId = value;
            _ = OnDeviceSelected(value);
        }
    }


    private Task HandleAddDiscount(decimal selected)
    {
        Discount = selected;
        Invoice.InvoicePrice = InvoicePrice;
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
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, ex.Message, MudBlazor.Severity.Warning);
            NoInvoice = true;
        }

    }

    private async Task OpenFilteredWatchers()
    {
        try
        {
            var result = await ShiftZoneActionAppService.GetShiftZoneDevicesDataAsync();
            _filteredWatchersByShiftZone = result?.Value ?? new List<ShiftWatcherActionDto>();
            // keep filtered watchers in a variable to use it in the UI even when the user click on refresh button to not make him select again the watcher 
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, ex.Message, MudBlazor.Severity.Error);
        }
    }

    private async Task OnDeviceSelected(string? value)
    {
        await LoadInvoiceForSelectedDevice();
    }

    private async Task LoadInvoiceForSelectedDevice()
    {
        if (string.IsNullOrWhiteSpace(SelectedDeviceId))
        {
            Invoice = new InvoiceDto();
            NoInvoice = true;
            StateHasChanged(); 
            return;
        }

        try
        {

            var result = await KeeperInvoiceAppService
                .GetLastInvoiceByOrganizationIdAndWatcherCodeAsync(_organization.Id, SelectedDeviceId);

            Invoice = result?.Value ?? new InvoiceDto();
            InvoicePrice = Invoice.InvoicePrice;


            NoInvoice = InvoicePrice == 0;
        }
        catch
        {
            Invoice = new InvoiceDto();
            NoInvoice = true;
        }
        finally
        {
            StateHasChanged(); 
        }
    }
    private async Task PayInvoice()
    {
        isPaying = true;
        if (Discount != default)
        {
            await InvoiceAppService.UpdateDiscountForInvoice(Invoice.Id, Discount);
        }
        try
        {
            if (PaymentMethod == PaymentMethod.POS)
            {

                var checkoutRequest = new PaymentCheckoutRequestDto
                {
                    OrganizationId = Invoice.OrganizationId,
                    PaymentCategory = PaymentCategory.PaymentParking,
                    InvoiceId = Invoice.Id,
                    PaymentMethod = PaymentMethod,
                    Amount = PaymentUtilityCalculationts.GetPriceForPayment(InvoicePrice, Discount),
                    CustomerId = Invoice.CustomerId,
                    PaymentName = PaymentMethod.POS.ToString(),
                    PaymentNumber = string.Empty // Generated inside SavePaymentRequest
                };

                Guid createdPaymentId = Guid.Empty;

                var parameters = new DialogParameters<PosPaymentDialog>
                {
                    { x => x.Request, checkoutRequest },
                    { x => x.OrgSettings, _organization!.OrganizationSettings },
                    { x => x.OnPaymentCreated, EventCallback.Factory.Create<Guid>(this, id => createdPaymentId = id) }
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
                        await OpenGate();
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
                PaymentCheckoutDto payment = await RequestToPaymentCheckout();
                if (payment != null)
                {
                    SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.ADD]}", MudBlazor.Severity.Success);
                    await Task.Delay(500);
                    await OpenGate();
                    await JSRuntime.InvokeVoidAsync("eval", $"window.open('/payments/view/{payment.PaymentId}', '_blank');");
                }
                else
                {
                    await OpenGate();
                }
            }
        }
        catch(Exception e)
        {
            Snackbar.Add(L[e.Message], MudBlazor.Severity.Error);
            await Task.Delay (1000);
        }
        finally
        {
            isPaying = false;
        }
        await Refresh();
    }
    private async Task OpenGate()
    {
        try
        {
            var OpenGateService =await OpenGateFactoryService.GetOpeningGateService(_organization);
            if (OpenGateService == null)
            {
                SnackBar.ShowSnackbar(Snackbar, L["NoWebSocketConnection"], MudBlazor.Severity.Error);
                await Task.Delay(500);
                return;
            }
            var result = await OpenGateService.OpenGateAsync(new OpenGateDto
            {
                OrganizationId = _organization.Id,
                WatcherCode = SelectedDeviceId
            });
            if (result == true)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L["OpenGate"]}", MudBlazor.Severity.Success);
                await Task.Delay(1000);
                Navigation.Refresh(true);
            }
            else
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.WARNING]} {L["ChooseGate"]}", MudBlazor.Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, ex.Message, MudBlazor.Severity.Error);
        }
    }
    private async Task Cancel()
    {
        try
        {
            var ExitTransctionId = Invoice.GarageTransactionExitId;
            if (Invoice.InvoiceType == InvoiceType.Paid.ToString())
            {
                Snackbar.Add("Invoice is already paid", MudBlazor.Severity.Success);
            }
            await InvoiceAppService.DeleteAsync(Invoice.Id);
            await GarageTransactionAppService.DeleteAsync(ExitTransctionId);
            SnackBar.ShowSnackbar(Snackbar, @L["Done"] + " " + @L["Delete"] + " " + @L["Invoice"], MudBlazor.Severity.Success);
            await Task.Delay(1000);
        }
        catch(Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, L[ex.Message], MudBlazor.Severity.Error);

        }
        await Refresh();
    }

    private async Task Refresh()
    {
        // keep filtered watchers in a variable to use it in the UI even when the user click on refresh button to not make him select again the watcher 
        // Re-load watchers
        await OpenFilteredWatchers();

        // Re-load the invoice for the *already selected* device
        await LoadInvoiceForSelectedDevice();
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
