using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.PlateColor;

namespace SmartParking.Blazor.Components.Pages.PlateColors
{
    partial class ViewPlateColorDialog
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialogAdd { get; set; }
        [Parameter] public CreateUpdatePlateColorDto PlateColor { get; set; }

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
