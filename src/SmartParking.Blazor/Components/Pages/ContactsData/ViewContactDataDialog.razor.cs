using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Organizations;

namespace SmartParking.Blazor.Components.Pages.ContactsData;

public partial class ViewContactDataDialog
{
    private MudForm form;

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }
    bool _clearable;
    [Parameter] public ContactInfoDto ContactDataDto { get; set; }


    protected override Task OnInitializedAsync()
    {
        return base.OnInitializedAsync();

    }
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
