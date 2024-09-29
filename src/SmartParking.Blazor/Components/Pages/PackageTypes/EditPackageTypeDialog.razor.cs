using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups;
using System.Threading.Tasks;

namespace SmartParking.Blazor.Components.Pages.PackageTypes;

public partial class EditPackageTypeDialog
{


    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public PackageTypeDto packageType { get; set; } = new PackageTypeDto();
    private MudForm editForm;
    bool _clearable;
    [Parameter] public int UpdatedId { get; set; }



    private async Task Save()
    {
        await editForm.Validate();
        if (editForm.IsValid)
        {

            // Create the input object for update
            var input = new CreateUpdatePackageTypeDto
            {
                ArabicName = packageType.ArabicName,
                EnglishName = packageType.EnglishName,
                Notes = packageType.Notes,
                Id = UpdatedId
            };

            // Call the update method
            await PackageTypesAppService.UpdateAsync(UpdatedId, input);

            // Close the dialog with a success result
            MudDialog.Close(DialogResult.Ok(true));
        }
    }


    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
