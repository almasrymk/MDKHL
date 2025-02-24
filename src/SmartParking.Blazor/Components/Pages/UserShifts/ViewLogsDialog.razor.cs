using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace SmartParking.Blazor.Components.Pages.UserShifts;

public partial class ViewLogsDialog
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }
    private void Cancel() => MudDialog.Cancel();
}
