using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Collections.Generic;
using System.Globalization;

namespace SmartParking.Blazor.Components.Pages.UserShifts;

public partial class ViewUserShiftsDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter] public string UserFullName { get; set; } = string.Empty;
    [Parameter] public string Role { get; set; } = string.Empty;
    [Parameter] public string Device { get; set; } = string.Empty;

    // All devices for Manager; empty for Staff
    [Parameter] public List<string> Devices { get; set; } = new();

    [Parameter] public string Garage { get; set; } = string.Empty;

    private void Cancel() => MudDialog.Cancel();

    private bool IsArabic => CultureInfo.CurrentCulture.Name == "ar";
    private bool IsManager =>
        string.Equals(Role, "Manager", StringComparison.OrdinalIgnoreCase);

    // Used by the Staff branch only
    private string SingleDeviceLabel =>
        string.IsNullOrWhiteSpace(Device) ? "—" : Device;
}