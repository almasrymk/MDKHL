using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.State;
using System.Threading.Tasks;
using Volo.Abp;

namespace SmartParking.Blazor.Components.Pages.States;

public partial class AddStateDialog
{
    private MudForm form;
    public CreateUpdateStateDto input = new CreateUpdateStateDto();

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }
    bool _clearable;
    private int RegionSelectValue { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    private void SelectRegion(int region)
    {
        RegionSelectValue = region;
    }
    private async Task Submit()
    {
        var successedMessage = L["StateCreatedSuccessfully"];
        var errorMessage = L["PleaseFixTheValidationErrors"];
        if (form.IsValid)
        {
            // Call your service to create the camera
            try
            {
                CreateUpdateStateDto createUpdateStateDto = new CreateUpdateStateDto()
                {
                    ArabicName = input.ArabicName,
                    EnglishName = input.EnglishName,
                    Status = input.Status,
                    RegionId = RegionSelectValue
                };
                await StateAppService.CreateAsync(createUpdateStateDto);
            }
            catch (UserFriendlyException ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
                return;
            }
            Snackbar.Add(successedMessage, Severity.Success);
            MudDialog.Close(DialogResult.Ok(true));

        }
        else
        {
            Snackbar.Add(errorMessage, Severity.Error);
        }
    }
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
