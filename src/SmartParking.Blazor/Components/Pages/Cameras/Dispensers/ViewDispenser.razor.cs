using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Cameras;
using SmartParking.Cameras.Watchers.Dispensers;

namespace SmartParking.Blazor.Components.Pages.Cameras.Dispensers;

public partial class ViewDispenser
{
    [CascadingParameter]
    private IMudDialogInstance MudDialogAdd { get; set; }
    [Parameter]
    public DispenserDto Dispenser { get; set; }
    [Parameter]
    public int UpdatedId { get; set; }

    private bool _clearable;
    private MudForm form;

    /// <summary>
    /// Closes the view-only dialog.
    /// </summary>
    private void OnCloseViewOnly()
    {
        MudDialogAdd.Cancel();
    }
}
