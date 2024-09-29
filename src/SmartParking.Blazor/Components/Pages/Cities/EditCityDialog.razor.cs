using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups;
using SmartParking.Lookups.City;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartParking.Blazor.Components.Pages.Cities;

public partial class EditCityDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public CityDto city { get; set; } = new CityDto();
    [Parameter] public CreateUpdateCityDto input { get; set; } = new CreateUpdateCityDto();
    public IReadOnlyList<GovernorateLookupDto> governorates { get; set; } = new List<GovernorateLookupDto>();
    private MudForm editForm;
    bool _clearable;
    [Parameter] public int UpdatedId { get; set; }


    protected override async Task OnInitializedAsync()
    {
        var result = await CitiesAppService.GetGovernorateLookupAsync();
        governorates = result;
    }

    private async Task Save()
    {
        await editForm.Validate();
        if (editForm.IsValid)
        {

            // Create the input object for update
            var input = new CreateUpdateCityDto
            {
                ArabicName = city.ArabicName,
                EnglishName = city.EnglishName,
                Notes = city.Notes,
                GovernorateId = city.GovernorateId,
                Id = UpdatedId
            };

            // Call the update method
            await CitiesAppService.UpdateAsync(UpdatedId, input);

            // Close the dialog with a success result
            MudDialog.Close(DialogResult.Ok(true));
        }
    }


    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
