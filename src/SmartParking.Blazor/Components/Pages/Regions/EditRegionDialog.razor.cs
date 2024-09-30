using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.Governorate;
using SmartParking.Lookups.Region;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartParking.Blazor.Components.Pages.Regions;

public partial class EditRegionDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public RegionDto region { get; set; } = new RegionDto();
    public IReadOnlyList<CountryLookupDto> countries { get; set; } = new List<CountryLookupDto>();
    private MudForm editForm;
    [Parameter] public int UpdatedId { get; set; }


    protected override async Task OnInitializedAsync()
    {
        var result = await GovernorateAppService.GetCountryLookupAsync();
        countries = result;
    }



    private async Task Save()
    {
        await editForm.Validate();
        if (editForm.IsValid)
        {

            // Create the input object for update
            var input = new CreateUpdateRegionDto
            {
                ArabicName = region.ArabicName,
                EnglishName = region.EnglishName,
                Status = region.Status,
                CountryId = region.CountryId,


            };

            // Call the update method
            await RegionAppService.UpdateAsync(UpdatedId, input);

            // Close the dialog with a success result
            MudDialog.Close(DialogResult.Ok(true));
        }
    }


    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
