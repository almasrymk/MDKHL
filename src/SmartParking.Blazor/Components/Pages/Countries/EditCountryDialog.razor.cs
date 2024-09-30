using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups;
using System.Threading.Tasks;

namespace SmartParking.Blazor.Components.Pages.Countries;

public partial class EditCountryDialog
{

    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public CountryDto country { get; set; } = new CountryDto();
    private MudForm editForm;
    [Parameter] public int UpdatedId { get; set; }

    private async Task Save()
    {
        await editForm.Validate();
        if (editForm.IsValid)
        {

            // Create the input object for update
            var input = new CreateUpdateCountryDto
            {
                ArabicName = country.ArabicName,
                EnglishName = country.EnglishName,
                ArabicCapital = country.ArabicCapital,
                EnglishCapital = country.EnglishCapital,
                Notes = country.Notes,
                Id = UpdatedId
            };

            // Call the update method
            await CountriesAppService.UpdateAsync(UpdatedId, input);

            // Close the dialog with a success result
            MudDialog.Close(DialogResult.Ok(true));
        }
    }


    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
