using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Cameras;
using SmartParking.CustomerPackages;
using SmartParking.Cutomers;
using SmartParking.Packages;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using static MudBlazor.CategoryTypes;
using Volo.Abp;
using Microsoft.Extensions.Localization;
using SmartParking.Localization;

namespace SmartParking.Blazor.Components.Pages.Customer;

public partial class CustomerVehicles
{
    [CascadingParameter]
    private MudDialogInstance MudDialogAdd { get; set; }

    [Parameter]
    public Guid CustomerId { get; set; }

    public List<VehicleDto> vehicles { get; set; } = new List<VehicleDto>();
    public List<CustomerPackageDto> Packages { get; set; } = new List<CustomerPackageDto>();
    public CustomerDto Customer = new CustomerDto();
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    protected override async Task OnInitializedAsync()
    {
        Customer = await CustomerAppService.GetAsync(CustomerId);
        vehicles = await CustomerAppService.GetVehiclesRelatedToSpecificCustomer(CustomerId);
        Packages = await CustomerPackagesAppService.GetListPackagesByCustomer(CustomerId);

    }
    public CustomerVehicles(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }
    private async Task DeletePackage(int id)
    {
        var dialog = await Dialog.ShowAsync<SPDialogDelete>("Delete Customer Package");
        var result = await dialog.Result;
        try
        {
            if (!result.Canceled)
            {
                await CustomerPackagesAppService.DeleteAsync(id);
                ShowSnackbar($"{L["customer package Deleted Successfully"]} ", Severity.Error);

                await Task.Delay(1000);

                Navigation.NavigateTo("/customers", forceLoad: true);

            }


        }
        catch (BusinessException ex)
        {

            ShowSnackbar($"Failed to delete camera: {ex.Code} + ", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            ShowSnackbar($"Failed to delet camera: {ex.Message} + ", Severity.Error);
        }
        catch (Exception ex)
        {

            ShowSnackbar($"Failed to delete camera: {ex.Message} + ", Severity.Error);
        }

    }
    /// <summary>

    /// Displays a snackbar notification with the specified message and severity.

    /// </summary>

    /// <param name="message">The message to display in the snackbar.</param>

    /// <param name="severity">The severity level of the snackbar (e.g., Success, Error).</param>

    void ShowSnackbar(string message, Severity severity)

    {

        Snackbar.Clear();
        Snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled);

    }
    private void OnCloseViewOnly()
    {
        MudDialogAdd.Cancel();

    }

}
