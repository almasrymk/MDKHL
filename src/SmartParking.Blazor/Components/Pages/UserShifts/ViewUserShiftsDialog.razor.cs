using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace SmartParking.Blazor.Components.Pages.UserShifts;

public partial class ViewUserShiftsDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter] public string UserFullName { get; set; } = string.Empty;
    [Parameter] public string Role { get; set; } = string.Empty;
    [Parameter] public string Device { get; set; } = string.Empty;

    [Parameter] public List<string> Devices { get; set; } = new();

    [Parameter] public string Garage { get; set; } = string.Empty;

    private void Cancel() => MudDialog.Cancel();
}