using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Mobile.Keepers.Dtos;
using SmartParking.OpeningGate;
using SmartParking.Organizations;
using SmartParking.Permissions.CheckoutPermissions;
using SmartParking.Permissions.UnlinkedEntryPermissions;
using SmartParking.Shifts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartParking.Blazor.Components.Pages.UnlinkedAtCamera;

public partial class UnlinkedTransactionAtCamera
{
    private OrganizationDto _organization = new OrganizationDto();
    private ICollection<ShiftWatcherActionDto> _filteredWatchersByShiftZone = new List<ShiftWatcherActionDto>();
    private UnlinkedTransactionViewDto? _unlinkedTransaction;

    private string editablePlateNumber = string.Empty;
    private string editableTicketNumber = string.Empty;
    private bool triggerGateOpen = true;
    private bool isLoading = false;
    private bool isSubmitting = false;

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

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _organization = await OrganizationAppService.GetOrganizationForCurrentTenatAsync();
            if (_organization?.OrganizationSettings?.UnlinkedEntry != true)
            {
                Navigation.NavigateTo("/Account/AccessDenied", true);
                return;
            }

            if (!await AuthorizationService.IsGrantedAsync(UnlinkedEntryPermissions.Default))
            {
                Navigation.NavigateTo("/Account/AccessDenied", true);
                return;
            }
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, ex.Message, Severity.Error);
        }

        await base.OnInitializedAsync();
    }

    private async Task OpenFilteredWatchersAsync()
    {
        try
        {
            var result = await ShiftZoneActionAppService.GetShiftZoneDevicesDataAsync();
            _filteredWatchersByShiftZone = result?.Value ?? new List<ShiftWatcherActionDto>();
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, ex.Message, Severity.Error);
        }
    }

    private async Task OnDeviceSelected(string? deviceId)
    {
        if (string.IsNullOrEmpty(deviceId))
        {
            _unlinkedTransaction = null;
            editablePlateNumber = string.Empty;
            editableTicketNumber = string.Empty;
            StateHasChanged();
            return;
        }

        await LoadUnlinkedTransactionForSelectedDeviceAsync();
    }

    private async Task LoadUnlinkedTransactionForSelectedDeviceAsync()
    {
        if (string.IsNullOrEmpty(SelectedDeviceId)) return;

        if (!await AuthorizationService.IsGrantedAsync(UnlinkedEntryPermissions.View))
        {
            SnackBar.ShowSnackbar(Snackbar, L["AccessDenied"], Severity.Warning);
            return;
        }

        isLoading = true;
        StateHasChanged();

        try
        {
            var result = await UnlinkedTransactionRecoveryAppService.GetLatestUnlinkedTransactionByWatcherCodeAsync(_organization.Id, SelectedDeviceId);
            if (result != null && result.IsSuccess && result.Value != null && !result.Value.NoTransaction)
            {
                _unlinkedTransaction = result.Value;
                editablePlateNumber = _unlinkedTransaction.PlateNumber ?? string.Empty;
                editableTicketNumber = _unlinkedTransaction.TicketNumber ?? string.Empty;
            }
            else
            {
                _unlinkedTransaction = null;
                editablePlateNumber = string.Empty;
                editableTicketNumber = string.Empty;
                if (result != null && !result.IsSuccess && result.ValidationErrors != null && result.ValidationErrors.Count > 0)
                {
                    SnackBar.ShowSnackbar(Snackbar, result.ValidationErrors[0].ErrorMessage, Severity.Warning);
                }
            }
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, ex.Message, Severity.Error);
            _unlinkedTransaction = null;
            editablePlateNumber = string.Empty;
            editableTicketNumber = string.Empty;
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private async Task RefreshAsync()
    {
        await OpenFilteredWatchersAsync();
        if (!string.IsNullOrEmpty(SelectedDeviceId))
        {
            await LoadUnlinkedTransactionForSelectedDeviceAsync();
        }
    }

    private async Task OpenGateAsync()
    {
        if (string.IsNullOrEmpty(SelectedDeviceId) || _unlinkedTransaction == null || _unlinkedTransaction.NoTransaction)
        {
            SnackBar.ShowSnackbar(Snackbar, L["NoTransactionSelected"], Severity.Warning);
            return;
        }

        try
        {
            var success = await KeeperTransactionsAppService.OpenGate(new OpenGateDto
            {
                WatcherCode = SelectedDeviceId,
                OrganizationId = _organization.Id
            });

            if (success)
            {
                SnackBar.ShowSnackbar(Snackbar, L["GateOpenedSuccessfully"], Severity.Success);
            }
            else
            {
                SnackBar.ShowSnackbar(Snackbar, L["FailedToOpenGate"], Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, ex.Message, Severity.Error);
        }
    }

    private async Task SendAsEntryAsync()
    {
        if (_unlinkedTransaction == null || _unlinkedTransaction.NoTransaction || string.IsNullOrEmpty(SelectedDeviceId))
        {
            SnackBar.ShowSnackbar(Snackbar, L["NoTransactionSelected"], Severity.Warning);
            return;
        }

        string ticket = (editableTicketNumber ?? string.Empty).Trim();
        string plate = (editablePlateNumber ?? string.Empty).Trim().Replace(" ", "");

        if (_organization.DispenserTransaction)
        {
            if (string.IsNullOrEmpty(ticket))
            {
                SnackBar.ShowSnackbar(Snackbar, L["TicketNumberRequired"], Severity.Warning);
                return;
            }
        }
        else
        {
            if (string.IsNullOrEmpty(plate))
            {
                SnackBar.ShowSnackbar(Snackbar, L["PlateNumberRequired"], Severity.Warning);
                return;
            }
        }

        bool? confirm = await DialogService.ShowMessageBox(
            L["ConfirmRecovery"],
            L["AreYouSureYouWantToSendAsEntry"],
            yesText: L["Yes"],
            cancelText: L["Cancel"]);

        if (confirm != true) return;

        isSubmitting = true;
        StateHasChanged();

        try
        {
            var input = new SendUnlinkedAsEntryDto
            {
                UnlinkedTransactionId = _unlinkedTransaction.Id,
                OrganizationId = _organization.Id,
                WatcherCode = SelectedDeviceId,
                CorrectedPlateNumber = string.IsNullOrEmpty(plate) ? null : plate,
                TicketNumber = string.IsNullOrEmpty(ticket) ? null : ticket,
                TriggerGateOpen = triggerGateOpen
            };

            var result = await UnlinkedTransactionRecoveryAppService.SendUnlinkedTransactionAsEntryAsync(input);

            if (result != null && result.IsSuccess)
            {
                SnackBar.ShowSnackbar(Snackbar, L["UnlinkedEntryCreatedSuccessfully"], Severity.Success);
                await LoadUnlinkedTransactionForSelectedDeviceAsync();
            }
            else
            {
                string errorMsg = result?.ValidationErrors != null && result.ValidationErrors.Count > 0
                    ? result.ValidationErrors[0].ErrorMessage
                    : result?.Errors?.FirstOrDefault() ?? L["FailedToAddUnlinkedEntry"];
                SnackBar.ShowSnackbar(Snackbar, errorMsg, Severity.Error);
            }
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, ex.Message, Severity.Error);
        }
        finally
        {
            isSubmitting = false;
            StateHasChanged();
        }
    }
}
