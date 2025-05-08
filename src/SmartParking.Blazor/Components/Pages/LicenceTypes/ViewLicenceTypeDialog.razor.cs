using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.LicenceType;

namespace SmartParking.Blazor.Components.Pages.LicenceTypes
{
    partial class ViewLicenceTypeDialog
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialogAdd { get; set; }
        [Parameter] public CreateUpdateLicenceTypeDto LicenceType { get; set; }

        bool _clearable;
        private MudForm form;

        protected override async Task OnInitializedAsync()
        {

            await base.OnInitializedAsync();
        }



        private void OnCloseViewOnly()
        {
            MudDialogAdd.Cancel();

        }

    }
}
