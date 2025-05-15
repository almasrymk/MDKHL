using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Organizations;

namespace SmartParking.Blazor.Components.Pages.ContactsData;

/// <summary>
/// Dialog for viewing contact data.
/// This component displays the details of a contact and allows users to cancel the dialog without making any changes.
/// </summary>
public partial class ViewContactDataDialog
{
    private MudForm form;
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }
    [Parameter] public ContactInfoDto ContactDataDto { get; set; }

    /// <summary>
    /// Initializes the component when it is first rendered.
    /// This method is called by the Blazor framework when the component is initialized.
    /// </summary>
    protected override Task OnInitializedAsync()
    {
        return base.OnInitializedAsync();
    }

    /// <summary>
    /// Cancels the dialog and closes it without making any changes to the contact data.
    /// </summary>
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}

