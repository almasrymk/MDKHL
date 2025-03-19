using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.State;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartParking.Blazor.Components.Pages.States;

public partial class DetailsStateDialog
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }
    [Parameter] public StateDto state { get; set; } = new StateDto();

    protected override async Task OnInitializedAsync()
    {
       await base.OnInitializedAsync();
    }
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
