using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.State;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartParking.Blazor.Components.Pages.States;

public partial class EditStateDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public StateDto state { get; set; } = new StateDto();
    [Parameter] public int UpdatedId { get; set; }
    public IReadOnlyList<RegionLookupDto> regions { get; set; } = new List<RegionLookupDto>();

    private MudForm editForm;


    protected override async Task OnInitializedAsync()
    {
        var result = await StateAppService.GetRegionLookupAsync();
        regions = result;
    }


    private async Task Save()
    {
        await editForm.Validate();
        if (editForm.IsValid)
        {

            // Create the input object for update
            var input = new CreateUpdateStateDto
            {
                ArabicName = state.ArabicName,
                EnglishName = state.EnglishName,
                Status = state.Status,
                RegionId = state.RegionId,


            };

            // Call the update method
            await StateAppService.UpdateAsync(UpdatedId, input);

            // Close the dialog with a success result
            MudDialog.Close(DialogResult.Ok(true));
        }
    }


    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
