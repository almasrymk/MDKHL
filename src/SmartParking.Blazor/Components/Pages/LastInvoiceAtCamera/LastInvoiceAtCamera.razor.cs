
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
    [Inject] private IInvoicePaymentProcessorAppService InvoicePaymentProcessorAppService { get; set; } = default!;

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
        if (Invoice == null || Invoice.Id == 0) return;

        isPaying = true;
        try
        {
            var initResult = await InvoicePaymentProcessorAppService.InitiatePaymentAsync(new ProcessInvoicePaymentRequestDto
            {
                InvoiceId = Invoice.Id,
                PaymentMethod = PaymentMethod,
                Discount = Discount
            });

            if (initResult.Result == InvoicePaymentResult.AlreadyPaid)
            {
                Snackbar.Add(L["InvoiceIsAlreadyPaid"], MudBlazor.Severity.Info);
                await Refresh();
                return;
            }

            if (initResult.Result == InvoicePaymentResult.PaymentInProgress)
            {
                Snackbar.Add(L["PaymentInProgress"], MudBlazor.Severity.Warning);
                await LoadInvoiceForSelectedDevice();
                return;
            }

            if (initResult.Result == InvoicePaymentResult.Failed)
            {
                Snackbar.Add(initResult.Message ?? L["PaymentFailed"], MudBlazor.Severity.Error);
                await LoadInvoiceForSelectedDevice();

                return;
            }

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
                    PaymentNumber = initResult.PaymentNumber ?? string.Empty
                };

                Guid createdPaymentId = initResult.PaymentId ?? Guid.Empty;

                var parameters = new DialogParameters<PosPaymentDialog>
                {
                    { x => x.Request, checkoutRequest },
                    { x => x.OrgSettings, _organization!.OrganizationSettings },
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
                        InvoiceId = Invoice.Id,
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
                        await OpenGate();
                        await JSRuntime.InvokeVoidAsync("eval", $"window.open('/payments/view/{createdPaymentId}', '_blank');");
                    }
                    else if (completeResult.Result == InvoicePaymentResult.AlreadyPaid)
                    {
                        Snackbar.Add(completeResult.Message ?? L["InvoiceIsAlreadyPaid"], MudBlazor.Severity.Warning);
                        await Refresh();
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
                    await OpenGate();
                    await JSRuntime.InvokeVoidAsync("eval", $"window.open('/payments/view/{initResult.PaymentId.Value}', '_blank');");
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


}
