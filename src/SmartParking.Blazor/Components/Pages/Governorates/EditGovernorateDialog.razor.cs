using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.Governorate;
using SmartParking.Lookups;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartParking.Blazor.Components.Pages.Governorates;

public partial class EditGovernorateDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public GovernorateDto governorate { get; set; } = new GovernorateDto();
    public IReadOnlyList<CountryLookupDto> countries { get; set; } = new List<CountryLookupDto>();
    private MudForm editForm;
    bool _clearable;
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
            var input = new CreateUpdateGovernorateDto
            {
                ArabicName = governorate.ArabicName,
                EnglishName = governorate.EnglishName,
                ArabicCapital = governorate.ArabicCapital,
                EnglishCapital = governorate.EnglishCapital,
                Notes = governorate.Notes,
                CountryId = governorate.CountryId,
                Id = UpdatedId

            };

            // Call the update method
            await GovernorateAppService.UpdateAsync(UpdatedId, input);

            // Close the dialog with a success result
            MudDialog.Close(DialogResult.Ok(true));
        }
    }


    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
