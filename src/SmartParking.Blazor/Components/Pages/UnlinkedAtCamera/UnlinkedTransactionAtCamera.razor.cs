using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Mobile.Keepers.Dtos;
using SmartParking.OpeningGate;
using SmartParking.Organizations;
using SmartParking.Permissions.CheckoutPermissions;
using SmartParking.Permissions.UnLinkedTransactionPermissions;
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
            if (!await AuthorizationService.IsGrantedAsync(UnLinkedTransactionPermission.Default) &&
                !await AuthorizationService.IsGrantedAsync(CheckoutPermissions.Default))
            {
                Navigation.NavigateTo("/Account/AccessDenied", true);
                return;
            }

            _organization = await OrganizationAppService.GetOrganizationForCurrentTenatAsync();
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
            StateHasChanged();
            return;
        }

        await LoadUnlinkedTransactionForSelectedDeviceAsync();
    }

    private async Task LoadUnlinkedTransactionForSelectedDeviceAsync()
    {
        if (string.IsNullOrEmpty(SelectedDeviceId)) return;

        isLoading = true;
        StateHasChanged();

        try
        {
            var result = await KeeperAppService.GetLatestUnlinkedTransactionByWatcherCodeAsync(_organization.Id, SelectedDeviceId);
            if (result != null && result.IsSuccess && result.Value != null && !result.Value.NoTransaction)
            {
                _unlinkedTransaction = result.Value;
                editablePlateNumber = _unlinkedTransaction.PlateNumber ?? string.Empty;
            }
            else
            {
                _unlinkedTransaction = null;
                editablePlateNumber = string.Empty;
            }
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, ex.Message, Severity.Error);
            _unlinkedTransaction = null;
            editablePlateNumber = string.Empty;
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
        if (string.IsNullOrEmpty(SelectedDeviceId)) return;

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
        if (_unlinkedTransaction == null || string.IsNullOrEmpty(SelectedDeviceId)) return;

        string plate = (editablePlateNumber ?? string.Empty).Trim().Replace(" ", "");
        if (string.IsNullOrEmpty(plate))
        {
            SnackBar.ShowSnackbar(Snackbar, L["PlateNumberRequired"], Severity.Warning);
            return;
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
                CorrectedPlateNumber = plate,
                TriggerGateOpen = triggerGateOpen
            };

            var result = await KeeperAppService.SendUnlinkedTransactionAsEntryAsync(input);

            if (result != null && result.IsSuccess)
            {
                SnackBar.ShowSnackbar(Snackbar, L["UnLinkedTransactionCreatedSuccessfully"], Severity.Success);
                await LoadUnlinkedTransactionForSelectedDeviceAsync();
            }
            else
            {
                string errorMsg = result?.ValidationErrors != null && result.ValidationErrors.Count > 0
                    ? result.ValidationErrors[0].ErrorMessage
                    : result?.Errors?.FirstOrDefault() ?? L["FailedToAddUnLinkedTransaction"];
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
